# Feedback Feature Documentation

## Overview
The feedback feature allows instructors and students to send feedback to administrators, who can then review, manage, and dismiss feedback entries.

## User Roles and Permissions

### Students & Instructors
- ✅ Create new feedback
- ✅ View their own feedback
- ✅ View individual feedback details (own feedback only)

### Administrators
- ✅ View all feedback
- ✅ View unresolved feedback
- ✅ View individual feedback details (any feedback)
- ✅ Mark feedback as resolved/unresolved
- ✅ Delete feedback (dismiss)

## API Endpoints

### 1. Create Feedback
- **Method**: `POST /api/feedback`
- **Roles**: Student, Instructor
- **Body**:
  ```json
  {
    "subject": "Issue Title",
    "message": "Detailed description"
  }
  ```

### 2. Get My Feedback
- **Method**: `GET /api/feedback/my`
- **Roles**: Student, Instructor
- **Returns**: List of user's own feedback

### 3. Get Single Feedback
- **Method**: `GET /api/feedback/{id}`
- **Roles**: Admin (any feedback), Student/Instructor (own feedback only)
- **Returns**: Detailed feedback information

### 4. Get All Feedback
- **Method**: `GET /api/feedback`
- **Roles**: Admin only
- **Returns**: List of all feedback entries

### 5. Get Unresolved Feedback
- **Method**: `GET /api/feedback/unresolved`
- **Roles**: Admin only
- **Returns**: List of unresolved feedback entries

### 6. Mark as Resolved
- **Method**: `PUT /api/feedback/{id}/resolve`
- **Roles**: Admin only
- **Action**: Marks feedback as resolved

### 7. Mark as Unresolved
- **Method**: `PUT /api/feedback/{id}/unresolve`
- **Roles**: Admin only
- **Action**: Marks feedback as unresolved

### 8. Delete Feedback
- **Method**: `DELETE /api/feedback/{id}`
- **Roles**: Admin only
- **Action**: Permanently deletes feedback

## Data Models

### Feedback Entity
```csharp
public class Feedback
{
    public int FeedbackId { get; set; }
    public string Subject { get; set; }           // Max 100 characters
    public string Message { get; set; }           // Max 1000 characters
    public DateTime CreatedAt { get; set; }
    public bool IsResolved { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
}
```

### DTOs

#### CreateFeedbackDto
```csharp
{
    "subject": "string (required, max 100 chars)",
    "message": "string (required, max 1000 chars)"
}
```

#### FeedbackDto (Detailed view)
```csharp
{
    "feedbackId": 1,
    "subject": "Issue Title",
    "message": "Detailed description",
    "createdAt": "2024-01-01T00:00:00Z",
    "isResolved": false,
    "userFullName": "John Doe",
    "userEmail": "john@example.com",
    "userRole": "Student"
}
```

#### FeedbackListDto (List view)
```csharp
{
    "feedbackId": 1,
    "subject": "Issue Title",
    "createdAt": "2024-01-01T00:00:00Z",
    "isResolved": false,
    "userFullName": "John Doe",
    "userRole": "Student"
}
```

## Database Changes

### Migration: AddFeedbackTable
- Creates `Feedbacks` table
- Establishes foreign key relationship with `AspNetUsers`
- Includes indexes for performance

### Applied Database Schema
```sql
CREATE TABLE Feedbacks (
    FeedbackId int IDENTITY(1,1) PRIMARY KEY,
    Subject nvarchar(100) NOT NULL,
    Message nvarchar(1000) NOT NULL,
    CreatedAt datetime2 NOT NULL,
    IsResolved bit NOT NULL DEFAULT 0,
    UserId nvarchar(450) NOT NULL,
    FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id) ON DELETE CASCADE
);

CREATE INDEX IX_Feedbacks_UserId ON Feedbacks (UserId);
```

## Security Features

1. **Role-based Authorization**: Each endpoint has appropriate role restrictions
2. **User Ownership**: Students and instructors can only view their own feedback
3. **Admin Privileges**: Admins have full control over all feedback
4. **Input Validation**: Subject and message have length restrictions
5. **Cascade Delete**: Feedback is automatically deleted if user is deleted

## Usage Examples

### Student/Instructor Creating Feedback
```http
POST /api/feedback
Authorization: Bearer {token}
Content-Type: application/json

{
  "subject": "System Performance Issue",
  "message": "The assessment page loads slowly when there are many questions."
}
```

### Admin Viewing Unresolved Feedback
```http
GET /api/feedback/unresolved
Authorization: Bearer {admin_token}
```

### Admin Resolving Feedback
```http
PUT /api/feedback/123/resolve
Authorization: Bearer {admin_token}
```

## Repository Pattern Implementation

The feedback feature follows the established repository pattern:
- `IFeedbackRepository` interface
- `FeedbackRepository` implementation
- Integration with `UnitOfWork` pattern
- Proper dependency injection setup

## Error Handling

- **400 Bad Request**: Invalid input data
- **401 Unauthorized**: Missing or invalid authentication
- **403 Forbidden**: Insufficient permissions
- **404 Not Found**: Feedback doesn't exist
- **Validation Errors**: Detailed field-level validation messages

## Testing

To apply the database migration:
```bash
dotnet ef database update
```

To test the endpoints, use the sample HTTP requests provided in the project or create your own test cases following the API documentation above.

## Files Created/Modified

### New Files
- `Models/Feedback.cs`
- `Dtos/Feedback/FeedbackDto.cs`
- `Repositories/Interfaces/IFeedbackRepository.cs`
- `Repositories/Implementations/FeedbackRepository.cs`
- `Controllers/FeedbackController.cs`
- `Migrations/AddFeedbackTable.cs`

### Modified Files
- `Data/PolarisDbContext.cs` - Added Feedback DbSet and configuration
- `Repositories/Interfaces/IUnitOfWork.cs` - Added Feedbacks property
- `Repositories/Implementations/UnitOfWork.cs` - Added Feedbacks repository 