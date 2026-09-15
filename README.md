# 🔗 Makachi Inventory API

**Secure, scalable backend for inventory management.**  
Makachi Inventory API is a RESTful backend service built with ASP.NET Core, designed to power the Makachi Inventory UI. It provides endpoints for managing products, categories, and users with built-in authentication to ensure secure access across the system.

## 🧩 Project Purpose

This API serves as the backend for Makachi inventory management system. It handles business logic, data persistence, and secure communication with the frontend UI.

## 🛠️ Tech Stack

- **Framework:** ASP.NET Core (.NET 9)
- **Language:** C#
- **Architecture:** RESTful API
- **Authentication:** JWT (JSON Web Tokens)
- **ORM:** Entity Framework Core
- **Database:** SQL Server
- **Documentation:** Swagger (OpenAPI)

## Architecture Notes

This project favors simplicity given its scope:

- **No service/repository layer** — `DbContext` is injected directly into controllers. 
  Because of its simplistic design, a service layer wasn't deemed necessary. Would introduce 
  one for a larger app or to enable easier unit testing.
  
## 🔐 Security Features

- 🔑 **JWT Authentication:** Secure token-based login system  
- 🧼 **Input Validation:** Prevents injection and malformed requests  
- 🔒 **Protected Endpoints:** Only authenticated users can access API routes  

## 📦 Core Endpoints

| Method | Endpoint                  | Description                          | Auth Required |
|--------|---------------------------|--------------------------------------|---------------|
| GET    | /api/products             | Retrieve all products                | ✅            |
| POST   | /api/products             | Add a new product                    | ✅            |
| PUT    | /api/products/{id}        | Update product details               | ✅            |
| DELETE | /api/products/{id}        | Remove a product                     | ✅            |
| GET    | /api/categories           | List all categories                  | ✅            |

## 📈 Business Value

- Centralized control over inventory data  
- Secure access for authenticated users  

## 🧪 Testing & Documentation
- 📘 Swagger UI for live API exploration  
- 🔄 Postman collection available for testing endpoints  

## 🚀 Future Enhancements
- 🔗 Supplier and invoice modules  
- 🌍 Localization support  

## 👨‍💻 About the Developer

Built by a developer focused on clean architecture, secure design, and scalable backend systems. This project reflects strong command of .NET Core, API development, and enterprise-grade authentication practices.



