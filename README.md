# Hotel Listing API

A robust, production-ready RESTful API built with **ASP.NET Core** for managing a hotel listing system. This project implements industry-standard design patterns, advanced security protocols, and cloud-native deployment strategies.

## 🚀 Overview

This API allows users to manage countries, hotels, and bookings. It is built with a focus on **Clean Architecture**, ensuring the codebase is maintainable, testable, and extendable.

### Key Features

* **Full CRUD Functionality:** Manage Countries, Hotels, and User Bookings.
* **Advanced Security:** Implementation of **ASP.NET Core Identity** and **JWT (JSON Web Tokens)** for secure authentication and role-based authorization.
* **Performance & Scaling:** Integrated **Caching**, **Rate Limiting**, and **Paging/Filtering** for optimized data retrieval.
* **Cloud Ready:** Fully configured for **Microsoft Azure** deployment with SQL Azure integration.

---

## 🏗 Architecture & Tech Stack

The project follows a **Layered Architecture** (Separation of Concerns) to decouple business logic from infrastructure.

### Technology Breakdown

* **Framework:** .NET 6+ / ASP.NET Core API
* **Database:** SQL Server using **Entity Framework Core (EF Core)**
* *Approach:* Code-First Development with Migrations.


* **Security:** JWT Authentication, Identity Core, and Custom Identity User fields.
* **Logging:** Centralized logging using **Serilog** and **Seq**.
* **Documentation:** Swagger/OpenAPI for interactive API testing.
* **Mapping:** AutoMapper for DTO (Data Transfer Object) management.

---

## 🛠 Advanced Implementations

* **Service Layer Pattern:** Logic is moved out of controllers into a dedicated service layer for cleaner, more testable code.
* **Global Exception Handling:** A centralized middleware to handle errors gracefully across the entire application.
* **API Versioning:** Support for multiple API versions to ensure backward compatibility.
* **Health Checks:** Monitoring endpoints to verify the API and Database status.
* **Validation:** Advanced Fluent Validation for incoming requests.

---

## 📂 Project Structure

Based on the repository's organization:

* `HotelListing.Api`: The entry point and UI/API layer.
* `HotelListing.Api.Application`: Business logic, DTOs, and Service interfaces.
* `HotelListing.Api.Domain`: Core entities and database models.
* `HotelListing.Api.Common`: Shared utilities and constants.

---

## 📜 Certification

This project was developed as part of a comprehensive Udemy specialization in **ASP.NET Core Web API Development**.

> **Course Focus:** Enterprise-level API design, JWT Security, EF Core Best Practices, and Azure DevOps.
![Udemy Certificate - ASP NET Course - Done on 31 Dec 2025](https://github.com/user-attachments/assets/0b2bb75c-cabe-4b78-906c-6688d9f6e273)


---

## 👤 Author

**Mohamed Mousa**

* GitHub: [@mohamedmousa77](https://www.google.com/search?q=https://github.com/mohamedmousa77)
* Portfolio: mohamedmousa.it
* LinkedIn: Mohamed Mousa
