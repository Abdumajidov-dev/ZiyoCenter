using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ZiyoMarket.Data.UnitOfWorks;
using ZiyoMarket.Domain.Entities.Support;
using ZiyoMarket.Domain.Enums;
using ZiyoMarket.Service.DTOs.Support;
using ZiyoMarket.Service.Extensions;
using ZiyoMarket.Service.Helpers;
using ZiyoMarket.Service.Interfaces;
using ZiyoMarket.Service.Results;

namespace ZiyoMarket.Service.Services;

/// <summary>
/// Support chat and messaging service implementation
/// </summary>
public class SupportService : ISupportService
{
	private readonly IUnitOfWork _unitOfWork;
	private readonly IMapper _mapper;
	private readonly INotificationService _notificationService;

	public SupportService(
		IUnitOfWork unitOfWork,
		IMapper mapper,
		INotificationService notificationService)
	{
		_unitOfWork = unitOfWork;
		_mapper = mapper;
		_notificationService = notificationService;
	}

	// ============ CHAT OPERATIONS ============

	public async Task<Result<SupportChatDetailDto>> GetChatByIdAsync(int chatId)
	{
		try
		{
			var chat = await _unitOfWork.SupportChats
				.SelectAsync(c => c.Id == chatId && !c.IsDeleted,
					new[] { "Customer", "Admin" });

			if (chat == null)
				return Result<SupportChatDetailDto>.NotFound("Chat not found");

			// Get messages
			var messages = await _unitOfWork.SupportMessages
				.SelectAll(m => m.ChatId == chatId && !m.IsDeleted)
				.OrderBy(m => m.CreatedAt)
				.ToListAsync();

			var dto = _mapper.Map<SupportChatDetailDto>(chat);
			dto.Messages = _mapper.Map<List<SupportMessageDto>>(messages);
			dto.MessageCount = messages.Count;
			dto.UnreadCount = messages.Count(m => !m.IsRead);

			return Result<SupportChatDetailDto>.Success(dto);
		}
		catch (Exception ex)
		{
			return Result<SupportChatDetailDto>.InternalError($"Error getting chat: {ex.Message}");
		}
	}

	public async Task<Result<PaginationResponse<SupportChatListDto>>> GetChatsAsync(
		SupportChatFilterRequest request)
	{
		try
		{
			var query = _unitOfWork.SupportChats
				.Table
				.Include(c => c.Customer)
				.Include(c => c.Admin)
				.Where(c => !c.IsDeleted);

			// Filters
			if (request.Status.HasValue)
				query = query.Where(c => c.Status == request.Status.Value);

			if (request.Priority is nameof(request.Priority))
				query = query.Where(c => c.Priority == request.Priority.ToString());

			if (request.CustomerId.HasValue)
				query = query.Where(c => c.CustomerId == request.CustomerId.Value);

			if (request.AdminId.HasValue)
				query = query.Where(c => c.AdminId == request.AdminId.Value);

			if (!string.IsNullOrWhiteSpace(request.SearchTerm))
			{
				var term = request.SearchTerm.ToLower();
				query = query.Where(c => c.Subject.ToLower().Contains(term));
			}

			if (!string.IsNullOrWhiteSpace(request.Category))
				query = query.Where(c => c.Category == request.Category);

			if (!string.IsNullOrWhiteSpace(request.Tag))
				query = query.Where(c => c.Tags.Contains(request.Tag));

			// Sorting
			query = request.SortBy?.ToLower() switch
			{
				"priority" => request.SortDescending
					? query.OrderByDescending(c => c.Priority)
					: query.OrderBy(c => c.Priority),
				"status" => request.SortDescending
					? query.OrderByDescending(c => c.Status)
					: query.OrderBy(c => c.Status),
				_ => query.OrderByDescending(c => c.StartedAt)
			};

			var total = await query.CountAsync();

			var chats = await query
				.Skip(request.Skip)
				.Take(request.PageSize)
				.ToListAsync();

			var dtos = _mapper.Map<List<SupportChatListDto>>(chats);

			// Add message counts
			foreach (var dto in dtos)
			{
				dto.MessageCount = await _unitOfWork.SupportMessages
					.CountAsync(m => m.ChatId == dto.Id && !m.IsDeleted);

				dto.UnreadCount = await _unitOfWork.SupportMessages
					.CountAsync(m => m.ChatId == dto.Id && !m.IsRead && !m.IsDeleted);
			}

			return Result<PaginationResponse<SupportChatListDto>>.Success(
				new PaginationResponse<SupportChatListDto>(dtos, total, request.PageNumber, request.PageSize));
		}
		catch (Exception ex)
		{
			return Result<PaginationResponse<SupportChatListDto>>.InternalError(
				$"Error getting chats: {ex.Message}");
		}
	}

	public async Task<Result<SupportChatDetailDto>> CreateChatAsync(CreateChatDto request, int customerId)
	{
		try
		{
			// Validate customer
			var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
			if (customer == null || !customer.IsActive)
				return Result<SupportChatDetailDto>.BadRequest("Invalid customer");

			// Check if order exists (if provided)
			if (request.OrderId.HasValue)
			{
				var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId.Value);
				if (order == null || order.CustomerId != customerId)
					return Result<SupportChatDetailDto>.BadRequest("Invalid order");
			}

			// Create chat
			var chat = new SupportChat
			{
				CustomerId = customerId,
				Subject = request.Subject,
				Priority = request.Priority ?? "Low", // Default: Low
				Status = SupportChatStatus.Open,
				OrderId = request.OrderId,
				Category = request.Category,
				Tags = request.Tags,
				StartedAt = TimeHelper.GetCurrentServerTime(),
			};

			await _unitOfWork.SupportChats.InsertAsync(chat);
			await _unitOfWork.SaveChangesAsync();

			// Send initial message if provided
			if (!string.IsNullOrWhiteSpace(request.InitialMessage))
			{
				var message = new SupportMessage
				{
					ChatId = chat.Id,
					SenderId = customerId,
					SenderType = UserType.Customer,
					Message = request.InitialMessage,
					AttachmentUrl = request.AttachmentUrl,
					IsRead = false
				};

				await _unitOfWork.SupportMessages.InsertAsync(message);
				await _unitOfWork.SaveChangesAsync();
			}

			// Notify admins about new chat
			await _notificationService.NotifyAdminsAboutNewSupportChatAsync(chat.Id);

			var result = await GetChatByIdAsync(chat.Id);
			return Result<SupportChatDetailDto>.Success(result.Data!, "Chat created successfully", 201);
		}
		catch (Exception ex)
		{
			return Result<SupportChatDetailDto>.InternalError($"Error creating chat: {ex.Message}");
		}
	}

	public async Task<Result> CloseChatAsync(int chatId, string closeReason, int closedBy)
	{
		try
		{
			var chat = await _unitOfWork.SupportChats.GetByIdAsync(chatId);
			if (chat == null)
				return Result.NotFound("Chat not found");

			if (chat.Status == SupportChatStatus.Closed)
				return Result.BadRequest("Chat is already closed");

			chat.Status = SupportChatStatus.Closed;
			chat.ClosedAt = TimeHelper.GetCurrentServerTime();
			chat.Resolution = closeReason;
			chat.UpdatedBy = closedBy;
			chat.MarkAsUpdated();

			await _unitOfWork.SupportChats.Update(chat, chat.Id);
			await _unitOfWork.SaveChangesAsync();

			// Notify customer
			await _notificationService.NotifyCustomerChatClosedAsync(chat.CustomerId, chatId);

			return Result.Success("Chat closed successfully");
		}
		catch (Exception ex)
		{
			return Result.InternalError($"Error closing chat: {ex.Message}");
		}
	}

	public async Task<Result> ReopenChatAsync(int chatId, int reopenedBy)
	{
		try
		{
			var chat = await _unitOfWork.SupportChats.GetByIdAsync(chatId);
			if (chat == null)
				return Result.NotFound("Chat not found");

			if (chat.Status != SupportChatStatus.Closed)
				return Result.BadRequest("Only closed chats can be reopened");

			chat.Status = SupportChatStatus.Open;
			chat.ClosedAt = null;
			chat.UpdatedBy = reopenedBy;
			chat.MarkAsUpdated();

			await _unitOfWork.SupportChats.Update(chat, chat.Id);
			await _unitOfWork.SaveChangesAsync();

			return Result.Success("Chat reopened successfully");
		}
		catch (Exception ex)
		{
			return Result.InternalError($"Error reopening chat: {ex.Message}");
		}
	}

	public async Task<Result> AssignChatAsync(int chatId, int adminId, int assignedBy)
	{
		try
		{
			var chat = await _unitOfWork.SupportChats.GetByIdAsync(chatId);
			if (chat == null)
				return Result.NotFound("Chat not found");

			var admin = await _unitOfWork.Admins.GetByIdAsync(adminId);
			if (admin == null || !admin.IsActive)
				return Result.BadRequest("Invalid admin");

			chat.AdminId = adminId;
			chat.Status = SupportChatStatus.InProgress;
			chat.UpdatedBy = assignedBy;
			chat.MarkAsUpdated();

			await _unitOfWork.SupportChats.Update(chat, chat.Id);
			await _unitOfWork.SaveChangesAsync();

			// Notify assigned admin
			await _notificationService.NotifyAdminChatAssignedAsync(adminId, chatId);

			return Result.Success("Chat assigned successfully");
		}
		catch (Exception ex)
		{
			return Result.InternalError($"Error assigning chat: {ex.Message}");
		}
	}

	public async Task<Result> UnassignChatAsync(int chatId, int unassignedBy)
	{
		try
		{
			var chat = await _unitOfWork.SupportChats.GetByIdAsync(chatId);
			if (chat == null)
				return Result.NotFound("Chat not found");

			chat.AdminId = null;
			chat.Status = SupportChatStatus.Open;
			chat.UpdatedBy = unassignedBy;
			chat.MarkAsUpdated();

			await _unitOfWork.SupportChats.Update(chat, chat.Id);
			await _unitOfWork.SaveChangesAsync();

			return Result.Success("Chat unassigned successfully");
		}
		catch (Exception ex)
		{
			return Result.InternalError($"Error unassigning chat: {ex.Message}");
		}
	}

	// ============ MESSAGE OPERATIONS ============

	public async Task<Result<List<SupportMessageDto>>> GetChatMessagesAsync(
		int chatId, int pageNumber = 1, int pageSize = 50)
	{
		try
		{
			var chat = await _unitOfWork.SupportChats.GetByIdAsync(chatId);
			if (chat == null)
				return Result<List<SupportMessageDto>>.NotFound("Chat not found");

			var skip = (pageNumber - 1) * pageSize;

			var messages = await _unitOfWork.SupportMessages
				.SelectAll(m => m.ChatId == chatId && !m.IsDeleted)
				.OrderBy(m => m.CreatedAt)
				.Skip(skip)
				.Take(pageSize)
				.ToListAsync();

			var dtos = _mapper.Map<List<SupportMessageDto>>(messages);

			return Result<List<SupportMessageDto>>.Success(dtos);
		}
		catch (Exception ex)
		{
			return Result<List<SupportMessageDto>>.InternalError($"Error getting messages: {ex.Message}");
		}
	}

	public async Task<Result<SupportMessageDto>> SendMessageAsync(
		SendMessageDto request, int senderId, string senderType)
	{
		try
		{
			var chat = await _unitOfWork.SupportChats
				.SelectAsync(c => c.Id == request.ChatId && !c.IsDeleted);

			if (chat == null)
				return Result<SupportMessageDto>.NotFound("Chat not found");

			// Validate sender
			if (senderType == "Customer" && chat.CustomerId != senderId)
				return Result<SupportMessageDto>.Forbidden("You can only send messages to your own chats");

			if (senderType == "Admin" && chat.AdminId != senderId && chat.AdminId != null)
				return Result<SupportMessageDto>.Forbidden("Chat is assigned to another admin");

			// Create message
			var message = new SupportMessage
			{
				ChatId = request.ChatId,
				SenderId = senderId,
				SenderType = Enum.Parse<UserType>(senderType),
				Message = request.Message,
				AttachmentUrl = request.AttachmentUrl,
				IsRead = false
			};

			await _unitOfWork.SupportMessages.InsertAsync(message);

			// Update chat status if needed
			if (chat.Status == SupportChatStatus.Open && senderType == "Admin")
			{
				chat.Status = SupportChatStatus.InProgress;
				chat.AdminId ??= senderId;
				chat.MarkAsUpdated();
				await _unitOfWork.SupportChats.Update(chat, chat.Id);
			}

			await _unitOfWork.SaveChangesAsync();

			// Send notification
			if (senderType == "Customer")
			{
				// Notify admin if assigned
				if (chat.AdminId.HasValue)
					await _notificationService.NotifyAdminNewMessageAsync(chat.AdminId.Value, chat.Id);
			}
			else
			{
				// Notify customer
				await _notificationService.NotifyCustomerNewMessageAsync(chat.CustomerId, chat.Id);
			}

			var dto = _mapper.Map<SupportMessageDto>(message);
			return Result<SupportMessageDto>.Success(dto, "Message sent successfully", 201);
		}
		catch (Exception ex)
		{
			return Result<SupportMessageDto>.InternalError($"Error sending message: {ex.Message}");
		}
	}

	public async Task<Result> DeleteMessageAsync(int messageId, int deletedBy)
	{
		try
		{
			var message = await _unitOfWork.SupportMessages.GetByIdAsync(messageId);
			if (message == null)
				return Result.NotFound("Message not found");

			message.deletedBy = deletedBy;
			message.Delete();

			await _unitOfWork.SupportMessages.Update(message, message.Id);
			await _unitOfWork.SaveChangesAsync();

			return Result.Success("Message deleted successfully");
		}
		catch (Exception ex)
		{
			return Result.InternalError($"Error deleting message: {ex.Message}");
		}
	}

	public async Task<Result> EditMessageAsync(int messageId, string newMessage, int editedBy)
	{
		try
		{
			var message = await _unitOfWork.SupportMessages.GetByIdAsync(messageId);
			if (message == null)
				return Result.NotFound("Message not found");

			if (message.SenderId != editedBy)
				return Result.Forbidden("You can only edit your own messages");

			message.Message = newMessage;
			message.UpdatedBy = editedBy;
			message.MarkAsUpdated();

			await _unitOfWork.SupportMessages.Update(message, message.Id);
			await _unitOfWork.SaveChangesAsync();

			return Result.Success("Message updated successfully");
		}
		catch (Exception ex)
		{
			return Result.InternalError($"Error updating message: {ex.Message}");
		}
	}

	public async Task<Result> MarkMessageAsReadAsync(int messageId, int userId)
	{
		try
		{
			var message = await _unitOfWork.SupportMessages.GetByIdAsync(messageId);
			if (message == null)
				return Result.NotFound("Message not found");

			if (message.IsRead)
				return Result.Success("Message already marked as read");

			message.IsRead = true;
			message.ReadAt = TimeHelper.GetCurrentServerTime();

			await _unitOfWork.SupportMessages.Update(message, message.Id);
			await _unitOfWork.SaveChangesAsync();

			return Result.Success("Message marked as read");
		}
		catch (Exception ex)
		{
			return Result.InternalError($"Error marking message as read: {ex.Message}");
		}
	}

	// ============ CUSTOMER SUPPORT ============

	public async Task<Result<List<SupportChatListDto>>> GetCustomerChatsAsync(int customerId)
	{
		try
		{
			var chats = await _unitOfWork.SupportChats
				.SelectAll(c => c.CustomerId == customerId && !c.IsDeleted, new[] { "Admin" })
				.OrderByDescending(c => c.StartedAt)
				.ToListAsync();

			var dtos = _mapper.Map<List<SupportChatListDto>>(chats);

			// Add message counts
			foreach (var dto in dtos)
			{
				dto.MessageCount = await _unitOfWork.SupportMessages
					.CountAsync(m => m.ChatId == dto.Id && !m.IsDeleted);

				dto.UnreadCount = await _unitOfWork.SupportMessages
					.CountAsync(m => m.ChatId == dto.Id && !m.IsRead &&
							   m.SenderType != UserType.Customer && !m.IsDeleted);
			}

			return Result<List<SupportChatListDto>>.Success(dtos);
		}
		catch (Exception ex)
		{
			return Result<List<SupportChatListDto>>.InternalError(
				$"Error getting customer chats: {ex.Message}");
		}
	}

	public async Task<Result<CustomerSupportStatsDto>> GetCustomerSupportStatsAsync(int customerId)
	{
		try
		{
			var chats = await _unitOfWork.SupportChats
				.SelectAll(c => c.CustomerId == customerId && !c.IsDeleted)
				.ToListAsync();

			var stats = new CustomerSupportStatsDto
			{
				TotalChats = chats.Count,
				OpenChats = chats.Count(c => c.Status == SupportChatStatus.Open),
				InProgressChats = chats.Count(c => c.Status == SupportChatStatus.InProgress),
				ClosedChats = chats.Count(c => c.Status == SupportChatStatus.Closed),
				AverageResponseTime = await CalculateAverageResponseTimeAsync(customerId),
				AverageResolutionTime = await CalculateAverageResolutionTimeAsync(customerId)
			};

			return Result<CustomerSupportStatsDto>.Success(stats);
		}
		catch (Exception ex)
		{
			return Result<CustomerSupportStatsDto>.InternalError(
				$"Error getting customer stats: {ex.Message}");
		}
	}

	public async Task<Result<SupportChatDetailDto>> GetLatestCustomerChatAsync(int customerId)
	{
		try
		{
			var chat = await _unitOfWork.SupportChats
				.SelectAll(c => c.CustomerId == customerId && !c.IsDeleted, new[] { "Admin" })
				.OrderByDescending(c => c.StartedAt)
				.FirstOrDefaultAsync();

			if (chat == null)
				return Result<SupportChatDetailDto>.NotFound("No chats found");

			return await GetChatByIdAsync(chat.Id);
		}
		catch (Exception ex)
		{
			return Result<SupportChatDetailDto>.InternalError(
				$"Error getting latest chat: {ex.Message}");
		}
	}

	// ============ ADMIN SUPPORT ============

	public async Task<Result<List<SupportChatListDto>>> GetAdminChatsAsync(int adminId)
	{
		try
		{
			var chats = await _unitOfWork.SupportChats
				.SelectAll(c => c.AdminId == adminId && !c.IsDeleted, new[] { "Customer" })
				.OrderByDescending(c => c.StartedAt)
				.ToListAsync();

			var dtos = _mapper.Map<List<SupportChatListDto>>(chats);

			foreach (var dto in dtos)
			{
				dto.MessageCount = await _unitOfWork.SupportMessages
					.CountAsync(m => m.ChatId == dto.Id && !m.IsDeleted);

				dto.UnreadCount = await _unitOfWork.SupportMessages
					.CountAsync(m => m.ChatId == dto.Id && !m.IsRead &&
							   m.SenderType == UserType.Customer && !m.IsDeleted);
			}

			return Result<List<SupportChatListDto>>.Success(dtos);
		}
		catch (Exception ex)
		{
			return Result<List<SupportChatListDto>>.InternalError(
				$"Error getting admin chats: {ex.Message}");
		}
	}

	public async Task<Result<AdminSupportStatsDto>> GetAdminSupportStatsAsync(int adminId)
	{
		try
		{
			var chats = await _unitOfWork.SupportChats
				.SelectAll(c => c.AdminId == adminId && !c.IsDeleted)
				.ToListAsync();

			var stats = new AdminSupportStatsDto
			{
				AssignedChats = chats.Count,
				OpenChats = chats.Count(c => c.Status == SupportChatStatus.Open),
				InProgressChats = chats.Count(c => c.Status == SupportChatStatus.InProgress),
				ClosedChats = chats.Count(c => c.Status == SupportChatStatus.Closed),
				AverageResponseTime = await CalculateAdminResponseTimeAsync(adminId),
				AverageResolutionTime = await CalculateAdminResolutionTimeAsync(adminId)
			};

			return Result<AdminSupportStatsDto>.Success(stats);
		}
		catch (Exception ex)
		{
			return Result<AdminSupportStatsDto>.InternalError(
				$"Error getting admin stats: {ex.Message}");
		}
	}

	public async Task<Result<List<SupportChatListDto>>> GetUnassignedChatsAsync()
	{
		try
		{
			var chats = await _unitOfWork.SupportChats
				.SelectAll(c => c.AdminId == null && c.Status != SupportChatStatus.Closed && !c.IsDeleted,
					new[] { "Customer" })
				.OrderByDescending(c => c.Priority)
				.ThenBy(c => c.StartedAt)
				.ToListAsync();

			var dtos = _mapper.Map<List<SupportChatListDto>>(chats);

			return Result<List<SupportChatListDto>>.Success(dtos);
		}
		catch (Exception ex)
		{
			return Result<List<SupportChatListDto>>.InternalError(
				$"Error getting unassigned chats: {ex.Message}");
		}
	}

	public async Task<Result<List<SupportChatListDto>>> GetOverdueChatsAsync()
	{
		try
		{
			var threshold = TimeHelper.GetCurrentServerTime().AddHours(-24);

			var chats = await _unitOfWork.SupportChats
				.SelectAll(c => c.Status != SupportChatStatus.Closed &&
							   DateTime.Parse(c.StartedAt.ToString()) < threshold && !c.IsDeleted,
					new[] { "Customer", "Admin" })
				.OrderBy(c => c.StartedAt)
				.ToListAsync();

			var dtos = _mapper.Map<List<SupportChatListDto>>(chats);

			return Result<List<SupportChatListDto>>.Success(dtos);
		}
		catch (Exception ex)
		{
			return Result<List<SupportChatListDto>>.InternalError(
				$"Error getting overdue chats: {ex.Message}");
		}
	}

	// ============ FEEDBACK ============

	public async Task<Result> SubmitChatFeedbackAsync(int chatId, SubmitFeedbackDto request)
	{
		try
		{
			var chat = await _unitOfWork.SupportChats.GetByIdAsync(chatId);
			if (chat == null)
				return Result.NotFound("Chat not found");

			if (chat.Status != SupportChatStatus.Closed)
				return Result.BadRequest("Can only rate closed chats");

			chat.CustomerRating = request.Rating;
			chat.CustomerFeedback = request.Feedback;
			chat.MarkAsUpdated();

			await _unitOfWork.SupportChats.Update(chat, chat.Id);
			await _unitOfWork.SaveChangesAsync();

			return Result.Success("Feedback submitted successfully");
		}
		catch (Exception ex)
		{
			return Result.InternalError($"Error submitting feedback: {ex.Message}");
		}
	}

	public async Task<Result<List<ChatFeedbackDto>>> GetChatFeedbackAsync(
		DateTime startDate, DateTime endDate)
	{
		try
		{
			var chats = await _unitOfWork.SupportChats
				.SelectAll(c => !c.IsDeleted &&
							   c.CustomerRating.HasValue &&
							   DateTime.Parse(c.ClosedAt.ToString()!) >= startDate &&
							   DateTime.Parse(c.ClosedAt.ToString()!) <= endDate,
					new[] { "Customer", "Admin" })
				.ToListAsync();

			var dtos = _mapper.Map<List<ChatFeedbackDto>>(chats);

			return Result<List<ChatFeedbackDto>>.Success(dtos);
		}
		catch (Exception ex)
		{
			return Result<List<ChatFeedbackDto>>.InternalError(
				$"Error getting feedback: {ex.Message}");
		}
	}

	public async Task<Result<FeedbackStatsDto>> GetFeedbackStatisticsAsync(
		DateTime startDate, DateTime endDate)
	{
		try
		{
			var chats = await _unitOfWork.SupportChats
				.SelectAll(c => !c.IsDeleted &&
							   c.CustomerRating.HasValue &&
							   DateTime.Parse(c.ClosedAt.ToString()!) >= startDate &&
							   DateTime.Parse(c.ClosedAt.ToString()!) <= endDate)
				.ToListAsync();

			var stats = new FeedbackStatsDto
			{
				TotalRatings = chats.Count,
				AverageRating = chats.Any() ? chats.Average(c => c.CustomerRating!.Value) : 0,
				Rating1Count = chats.Count(c => c.CustomerRating == 1),
				Rating2Count = chats.Count(c => c.CustomerRating == 2),
				Rating3Count = chats.Count(c => c.CustomerRating == 3),
				Rating4Count = chats.Count(c => c.CustomerRating == 4),
				Rating5Count = chats.Count(c => c.CustomerRating == 5)
			};

			return Result<FeedbackStatsDto>.Success(stats);
		}
		catch (Exception ex)
		{
			return Result<FeedbackStatsDto>.InternalError(
				$"Error getting feedback statistics: {ex.Message}");
		}
	}

	// ============ STATISTICS ============

	public async Task<Result<SupportStatsDto>> GetSupportStatisticsAsync(
		DateTime startDate, DateTime endDate)
	{
		try
		{
			var chats = await _unitOfWork.SupportChats
				.SelectAll(c => !c.IsDeleted &&
							   DateTime.Parse(c.StartedAt.ToString()) >= startDate &&
							   DateTime.Parse(c.StartedAt.ToString()) <= endDate)
				.ToListAsync();

			var stats = new SupportStatsDto
			{
				TotalChats = chats.Count,
				OpenChats = chats.Count(c => c.Status == SupportChatStatus.Open),
				InProgressChats = chats.Count(c => c.Status == SupportChatStatus.InProgress),
				ClosedChats = chats.Count(c => c.Status == SupportChatStatus.Closed),
				EscalatedChats = chats.Count(c => c.Status == SupportChatStatus.Escalated),
				AverageResponseTime = await CalculateOverallResponseTimeAsync(startDate, endDate),
				AverageResolutionTime = await CalculateOverallResolutionTimeAsync(startDate, endDate),
				CustomerSatisfactionScore = chats.Any(c => c.CustomerRating.HasValue)
					? chats.Where(c => c.CustomerRating.HasValue).Average(c => c.CustomerRating!.Value)
					: 0
			};

			return Result<SupportStatsDto>.Success(stats);
		}
		catch (Exception ex)
		{
			return Result<SupportStatsDto>.InternalError(
				$"Error getting support statistics: {ex.Message}");
		}
	}

	public async Task<Result<List<ChatResponseTimeDto>>> GetResponseTimeStatsAsync(
		DateTime startDate, DateTime endDate)
	{
		try
		{
			var chats = await _unitOfWork.SupportChats
				.SelectAll(c => !c.IsDeleted &&
							   DateTime.Parse(c.StartedAt.ToString()) >= startDate &&
							   DateTime.Parse(c.StartedAt.ToString()) <= endDate,
					new[] { "Admin" })
				.ToListAsync();

			var stats = new List<ChatResponseTimeDto>();

			foreach (var chat in chats)
			{
				var firstAdminMessage = await _unitOfWork.SupportMessages
					.SelectAsync(m => m.ChatId == chat.Id &&
									 m.SenderType == UserType.Admin &&
									 !m.IsDeleted);

				if (firstAdminMessage != null)
				{
					var startTime = DateTime.Parse(chat.StartedAt.ToString());
					var responseTime = DateTime.Parse(firstAdminMessage.CreatedAt);
					var duration = (responseTime - startTime).TotalMinutes;

					stats.Add(new ChatResponseTimeDto
					{
						ChatId = chat.Id,
						Subject = chat.Subject,
						AdminName = chat.Admin != null
							? $"{chat.Admin.Username} "
							: "Unassigned",
						ResponseTimeMinutes = duration
					});
				}
			}

			return Result<List<ChatResponseTimeDto>>.Success(stats);
		}
		catch (Exception ex)
		{
			return Result<List<ChatResponseTimeDto>>.InternalError(
				$"Error getting response time stats: {ex.Message}");
		}
	}

	public async Task<Result<List<ChatResolutionTimeDto>>> GetResolutionTimeStatsAsync(
		DateTime startDate, DateTime endDate)
	{
		try
		{
			var chats = await _unitOfWork.SupportChats
				.SelectAll(c => !c.IsDeleted &&
							   c.Status == SupportChatStatus.Closed &&
							   DateTime.Parse(c.StartedAt.ToString()) >= startDate &&
							   DateTime.Parse(c.StartedAt.ToString()) <= endDate,
					new[] { "Admin" })
				.ToListAsync();

			var stats = chats.Select(chat =>
			{
				var startTime = DateTime.Parse(chat.StartedAt.ToString());
				var closeTime = DateTime.Parse(chat.StartedAt.ToString()!);
				var duration = (closeTime - startTime).TotalHours;

				return new ChatResolutionTimeDto
				{
					ChatId = chat.Id,
					Subject = chat.Subject,
					AdminName = chat.Admin != null
						? $"{chat.Admin.Username} "
						: "Unassigned",
					ResolutionTimeHours = duration,
					CustomerRating = chat.CustomerRating
				};
			}).ToList();

			return Result<List<ChatResolutionTimeDto>>.Success(stats);
		}
		catch (Exception ex)
		{
			return Result<List<ChatResolutionTimeDto>>.InternalError(
				$"Error getting resolution time stats: {ex.Message}");
		}
	}

	// ============ TAGS AND CATEGORIES ============

	public async Task<Result<List<string>>> GetAllChatTagsAsync()
	{
		try
		{
			var chats = await _unitOfWork.SupportChats
				.SelectAll(c => !c.IsDeleted && !string.IsNullOrEmpty(c.Tags))
				.ToListAsync();

			var tags = chats
				.SelectMany(c => c.Tags!.Split(','))
				.Select(t => t.Trim())
				.Distinct()
				.OrderBy(t => t)
				.ToList();

			return Result<List<string>>.Success(tags);
		}
		catch (Exception ex)
		{
			return Result<List<string>>.InternalError($"Error getting tags: {ex.Message}");
		}
	}

	public async Task<Result<List<string>>> GetAllChatCategoriesAsync()
	{
		try
		{
			var categories = await _unitOfWork.SupportChats
				.SelectAll(c => !c.IsDeleted && !string.IsNullOrEmpty(c.Category))
				.Select(c => c.Category!)
				.Distinct()
				.OrderBy(c => c)
				.ToListAsync();

			return Result<List<string>>.Success(categories);
		}
		catch (Exception ex)
		{
			return Result<List<string>>.InternalError($"Error getting categories: {ex.Message}");
		}
	}

	public async Task<Result<List<SupportChatListDto>>> GetChatsByTagAsync(string tag)
	{
		try
		{
			var chats = await _unitOfWork.SupportChats
				.SelectAll(c => !c.IsDeleted && c.Tags != null && c.Tags.Contains(tag),
					new[] { "Customer", "Admin" })
				.OrderByDescending(c => c.StartedAt)
				.ToListAsync();

			var dtos = _mapper.Map<List<SupportChatListDto>>(chats);

			return Result<List<SupportChatListDto>>.Success(dtos);
		}
		catch (Exception ex)
		{
			return Result<List<SupportChatListDto>>.InternalError(
				$"Error getting chats by tag: {ex.Message}");
		}
	}

	public async Task<Result<List<SupportChatListDto>>> GetChatsByCategoryAsync(string category)
	{
		try
		{
			var chats = await _unitOfWork.SupportChats
				.SelectAll(c => !c.IsDeleted && c.Category == category,
					new[] { "Customer", "Admin" })
				.OrderByDescending(c => c.StartedAt)
				.ToListAsync();

			var dtos = _mapper.Map<List<SupportChatListDto>>(chats);

			return Result<List<SupportChatListDto>>.Success(dtos);
		}
		catch (Exception ex)
		{
			return Result<List<SupportChatListDto>>.InternalError(
				$"Error getting chats by category: {ex.Message}");
		}
	}

	// ============ BULK OPERATIONS ============

	public async Task<Result> DeleteAllChatsAsync(
		int deletedBy, DateTime? startDate = null, DateTime? endDate = null)
	{
		try
		{
			var query = _unitOfWork.SupportChats.SelectAll(c => !c.IsDeleted);

			if (startDate.HasValue)
				query = query.Where(c => DateTime.Parse(c.StartedAt.ToString()) >= startDate.Value);

			if (endDate.HasValue)
				query = query.Where(c => DateTime.Parse(c.StartedAt.ToString()) <= endDate.Value);

			var chats = await query.ToListAsync();

			foreach (var chat in chats)
			{
				chat.DeletedBy = deletedBy;
				chat.Delete();
				await _unitOfWork.SupportChats.Update(chat, chat.Id);
			}

			await _unitOfWork.SaveChangesAsync();

			return Result.Success($"{chats.Count} chats deleted successfully");
		}
		catch (Exception ex)
		{
			return Result.InternalError($"Error deleting chats: {ex.Message}");
		}
	}

	public async Task<Result<List<SupportChatDetailDto>>> SeedMockChatsAsync(int createdBy, int count = 10)
	{
		try
		{
			var customers = await _unitOfWork.Customers
				.SelectAll(c => c.IsActive && !c.IsDeleted)
				.Take(count)
				.ToListAsync();

			if (!customers.Any())
				return Result<List<SupportChatDetailDto>>.BadRequest("No active customers found");

			var mockChats = new List<SupportChat>();
			var random = new Random();

			var subjects = new[]
			{
				"Order delivery issue",
				"Product quality concern",
				"Payment problem",
				"Account access issue",
				"General inquiry",
				"Refund request",
				"Product recommendation",
				"Technical support needed"
			};

			var categories = new[] { "Orders", "Products", "Payments", "Account", "General" };

			foreach (var customer in customers.Take(count))
			{
				var chat = new SupportChat
				{
					CustomerId = customer.Id,
					Subject = subjects[random.Next(subjects.Length)],
					Priority = "",
					Status = (SupportChatStatus)random.Next(0, 4),
					Category = categories[random.Next(categories.Length)],
					StartedAt = TimeHelper.GetCurrentServerTime().AddDays(-random.Next(1, 30)),
					CreatedBy = createdBy
				};

				mockChats.Add(chat);
				await _unitOfWork.SupportChats.InsertAsync(chat);
			}

			await _unitOfWork.SaveChangesAsync();

			var result = new List<SupportChatDetailDto>();
			foreach (var chat in mockChats)
			{
				var dto = await GetChatByIdAsync(chat.Id);
				if (dto.IsSuccess)
					result.Add(dto.Data!);
			}

			return Result<List<SupportChatDetailDto>>.Success(result,
				$"{count} mock chats created successfully", 201);
		}
		catch (Exception ex)
		{
			return Result<List<SupportChatDetailDto>>.InternalError(
				$"Error seeding mock chats: {ex.Message}");
		}
	}

	public async Task<Result<List<SupportMessageDto>>> SeedMockMessagesAsync(
		int chatId, int createdBy, int count = 10)
	{
		try
		{
			var chat = await _unitOfWork.SupportChats
				.SelectAsync(c => c.Id == chatId && !c.IsDeleted, new[] { "Customer" });

			if (chat == null)
				return Result<List<SupportMessageDto>>.NotFound("Chat not found");

			var mockMessages = new List<SupportMessage>();
			var random = new Random();

			var customerMessages = new[]
			{
				"Hello, I need help with my order",
				"Can you check the status?",
				"When will my order arrive?",
				"I haven't received my product yet",
				"Thank you for your help"
			};

			var adminMessages = new[]
			{
				"Hello! How can I help you today?",
				"Let me check that for you",
				"Your order is on the way",
				"I've updated your order status",
				"Is there anything else I can help with?"
			};

			for (int i = 0; i < count; i++)
			{
				var isCustomerMessage = i % 2 == 0;
				var message = new SupportMessage
				{
					ChatId = chatId,
					SenderId = isCustomerMessage ? chat.CustomerId : createdBy,
					SenderType = isCustomerMessage ? UserType.Customer : UserType.Admin,
					Message = isCustomerMessage
						? customerMessages[random.Next(customerMessages.Length)]
						: adminMessages[random.Next(adminMessages.Length)],
					IsRead = random.Next(0, 2) == 1
				};

				mockMessages.Add(message);
				await _unitOfWork.SupportMessages.InsertAsync(message);
			}

			await _unitOfWork.SaveChangesAsync();

			var dtos = _mapper.Map<List<SupportMessageDto>>(mockMessages);

			return Result<List<SupportMessageDto>>.Success(dtos,
				$"{count} mock messages created successfully", 201);
		}
		catch (Exception ex)
		{
			return Result<List<SupportMessageDto>>.InternalError(
				$"Error seeding mock messages: {ex.Message}");
		}
	}

	// ============ HELPER METHODS ============

	private async Task<double> CalculateAverageResponseTimeAsync(int customerId)
	{
		var chats = await _unitOfWork.SupportChats
			.SelectAll(c => c.CustomerId == customerId && !c.IsDeleted)
			.ToListAsync();

		if (!chats.Any()) return 0;

		var responseTimes = new List<double>();

		foreach (var chat in chats)
		{
			var firstAdminMessage = await _unitOfWork.SupportMessages
				.SelectAsync(m => m.ChatId == chat.Id &&
								 m.SenderType == UserType.Admin &&
								 !m.IsDeleted);

			if (firstAdminMessage != null)
			{
				var startTime = DateTime.Parse(chat.StartedAt.ToString());
				var responseTime = DateTime.Parse(firstAdminMessage.CreatedAt);
				responseTimes.Add((responseTime - startTime).TotalMinutes);
			}
		}

		return responseTimes.Any() ? responseTimes.Average() : 0;
	}

	private async Task<double> CalculateAverageResolutionTimeAsync(int customerId)
	{
		var closedChats = await _unitOfWork.SupportChats
			.SelectAll(c => c.CustomerId == customerId &&
						   c.Status == SupportChatStatus.Closed &&
						   !c.IsDeleted)
			.ToListAsync();

		if (!closedChats.Any()) return 0;

		var resolutionTimes = closedChats.Select(c =>
		{
			var startTime = DateTime.Parse(c.StartedAt.ToString());
			var closeTime = DateTime.Parse(c.ClosedAt.ToString()!);
			return (closeTime - startTime).TotalHours;
		}).ToList();

		return resolutionTimes.Average();
	}

	private async Task<double> CalculateAdminResponseTimeAsync(int adminId)
	{
		var chats = await _unitOfWork.SupportChats
			.SelectAll(c => c.AdminId == adminId && !c.IsDeleted)
			.ToListAsync();

		if (!chats.Any()) return 0;

		var responseTimes = new List<double>();

		foreach (var chat in chats)
		{
			var firstAdminMessage = await _unitOfWork.SupportMessages
				.SelectAsync(m => m.ChatId == chat.Id &&
								 m.SenderId == adminId &&
								 m.SenderType == UserType.Admin &&
								 !m.IsDeleted);

			if (firstAdminMessage != null)
			{
				var startTime = DateTime.Parse(chat.StartedAt.ToString());
				var responseTime = DateTime.Parse(firstAdminMessage.CreatedAt);
				responseTimes.Add((responseTime - startTime).TotalMinutes);
			}
		}

		return responseTimes.Any() ? responseTimes.Average() : 0;
	}

	private async Task<double> CalculateAdminResolutionTimeAsync(int adminId)
	{
		var closedChats = await _unitOfWork.SupportChats
			.SelectAll(c => c.AdminId == adminId &&
						   c.Status == SupportChatStatus.Closed &&
						   !c.IsDeleted)
			.ToListAsync();

		if (!closedChats.Any()) return 0;

		var resolutionTimes = closedChats.Select(c =>
		{
			var startTime = DateTime.Parse(c.StartedAt.ToString());
			var closeTime = DateTime.Parse(c.ClosedAt.ToString()!);
			return (closeTime - startTime).TotalHours;
		}).ToList();

		return resolutionTimes.Average();
	}

	private async Task<double> CalculateOverallResponseTimeAsync(DateTime startDate, DateTime endDate)
	{
		var chats = await _unitOfWork.SupportChats
			.SelectAll(c => !c.IsDeleted &&
						   DateTime.Parse(c.StartedAt.ToString()) >= startDate &&
						   DateTime.Parse(c.StartedAt.ToString()) <= endDate)
			.ToListAsync();

		if (!chats.Any()) return 0;

		var responseTimes = new List<double>();

		foreach (var chat in chats)
		{
			var firstAdminMessage = await _unitOfWork.SupportMessages
				.SelectAsync(m => m.ChatId == chat.Id &&
								 m.SenderType == UserType.Admin &&
								 !m.IsDeleted);

			if (firstAdminMessage != null)
			{
				var startTime = DateTime.Parse(chat.StartedAt.ToString());
				var responseTime = DateTime.Parse(firstAdminMessage.CreatedAt);
				responseTimes.Add((responseTime - startTime).TotalMinutes);
			}
		}

		return responseTimes.Any() ? responseTimes.Average() : 0;
	}

	private async Task<double> CalculateOverallResolutionTimeAsync(DateTime startDate, DateTime endDate)
	{
		var closedChats = await _unitOfWork.SupportChats
			.SelectAll(c => !c.IsDeleted &&
						   c.Status == SupportChatStatus.Closed &&
						   DateTime.Parse(c.StartedAt.ToString()) >= startDate &&
						   DateTime.Parse(c.StartedAt.ToString()) <= endDate)
			.ToListAsync();

		if (!closedChats.Any()) return 0;

		var resolutionTimes = closedChats.Select(c =>
		{
			var startTime = DateTime.Parse(c.StartedAt.ToString());
			var closeTime = DateTime.Parse(c.ClosedAt.ToString()!);
			return (closeTime - startTime).TotalHours;
		}).ToList();

		return resolutionTimes.Average();
	}
}