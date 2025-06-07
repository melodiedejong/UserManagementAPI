# UserManagementAPI

A minimal ASP.NET Core Web API for managing users with authentication, input validation, error handling, and in-memory storage.

## Features

- **User CRUD**: Create, read, update, and delete users.
- **Authentication**: Bearer token authentication middleware.
- **Input Validation**: Validates required fields, email format, and max length.
- **Duplicate Checks**: Prevents duplicate usernames and emails.
- **Thread Safety**: Uses locking for in-memory data structures.
- **Error Handling**: Global middleware returns consistent JSON error responses.
- **Request/Response Logging**: Logs HTTP requests and responses to the console.
- **Test Endpoint**: `/throw` endpoint to test error handling.

## Requirements

- [.NET 6.0](https://dotnet.microsoft.com/download) or later

## Getting Started

1. **Clone the repository:**
   ```sh
   git clone https://github.com/<your-username>/UserManagementAPI.git
   cd UserManagementAPI
   ```

2. **Run the API:**
   ```sh
   dotnet run
   ```

3. **Test Endpoints:**
   - Use [REST Client](https://marketplace.visualstudio.com/items?itemName=humao.rest-client) in VS Code, [Postman](https://www.postman.com/), or `curl`.
   - Example request with authentication:
     ```
     GET http://localhost:5114/users
     Authorization: Bearer mysecrettoken123
     ```

## API Endpoints

| Method | Endpoint                        | Description                        | Auth Required |
|--------|---------------------------------|------------------------------------|--------------|
| GET    | `/`                             | Root endpoint                      | No           |
| GET    | `/users`                        | Get all users                      | Yes          |
| GET    | `/users/{id}`                   | Get user by ID                     | Yes          |
| GET    | `/users/by-username/{username}` | Get user by username               | Yes          |
| POST   | `/users`                        | Create a new user                  | Yes          |
| PUT    | `/users/{id}`                   | Update user by ID                  | Yes          |
| DELETE | `/users/{id}`                   | Delete user by ID                  | Yes          |
| GET    | `/throw`                        | Throws an exception (test error)   | Yes          |

## Authentication

- All endpoints (except `/`) require a valid Bearer token.
- Example tokens (for demo/testing):
  - `mysecrettoken123`
  - `another-valid-token`
- Add the header to your requests:
  ```
  Authorization: Bearer mysecrettoken123
  ```

## Example User JSON

```json
{
  "userName": "Tyler",
  "email": "housecat@email.com",
  "age": 23
}
```

## Notes

- **In-memory storage**: All data is lost when the app stops.
- **For production**: Use persistent storage, secure token management, and add rate limiting.

## License

MIT License

---

**Author:** [Melodie de Jong]
