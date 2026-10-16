# 🚢 Tracking — Import & Shipment Tracking System

> A system that lets a company follow every imported shipment — from the moment a customer asks to import a product until the container arrives — and pay for it online.

---

## 🧐 What is this, in plain words?

Imagine you want to import goods from another country. Normally you call a shipping company again and again asking:
*"Where is my container now?"*, *"How much will customs cost?"*, *"Did my payment go through?"*

This project is the **back-end (the "brain")** of a website/app that answers all of that in one place:

- 📝 You **submit an import request** for the product you want.
- 💰 The system **calculates the full cost** for you (shipping, customs, fees).
- 💳 You **pay online** by card (via Stripe).
- 📍 You **track your container** stage by stage until it arrives.
- 🔔 You get **notifications and messages** whenever something changes.

## 👥 Who uses it?

| Who | What they can do |
|---|---|
| **Customer** | Create import requests, see cost estimates, pay, track containers, leave reviews |
| **Company staff / Admin** | Manage products and containers, update shipment stages and customs status, review requests |

## 🔄 How it works (the simple story)

```
Customer asks to import ➜ System calculates cost ➜ Customer pays (Stripe)
        ➜ Container is shipped ➜ Stages updated (port, customs, delivery) ➜ Customer is notified
```

---

## 🛠️ For developers

**Tech stack:** ASP.NET Core Web API (.NET 10) · Clean Architecture · CQRS with MediatR · Entity Framework Core + SQL Server · JWT authentication · Stripe payments & webhooks · AutoMapper · Serilog · Swagger · Unit & integration tests

**Project structure**

| Folder | Role |
|---|---|
| `Domain/` | Core business entities (Container, ImportRequest, CostCalculation, Payment, Tracking, ...) |
| `Application/` | Use cases — commands & queries (CQRS / MediatR) |
| `Infrastructure/` | Database (EF Core), external services (Stripe, email, ...) |
| `APi_Deploy/` | The Web API: controllers, auth, startup |
| `ECommerce.UnitTests/`, `ECommerce.IntegrationTests/` | Automated tests |

**Run locally**

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download) and SQL Server.
2. In `APi_Deploy/appsettings.json`, replace every `CHANGE_ME` with your own values (DB connection string, JWT secret, Stripe keys).
   > Prefer `dotnet user-secrets` so real secrets never get committed.
3. Run:
   ```bash
   dotnet ef database update --project Infrastructure --startup-project APi_Deploy
   dotnet run --project APi_Deploy
   ```
4. Open `/swagger` in the browser to try the API.

The front-end for this system lives in **[Front_Tracking](https://github.com/HossamTohamy1/Front_Tracking)**.

---

## 🇪🇬 بالعربي

نظام بيخلّي العميل يتابع شحنته المستوردة خطوة بخطوة: يقدّم طلب استيراد، والنظام يحسبله التكلفة كاملة (شحن وجمارك ومصاريف)، يدفع أونلاين بالكارت، وبعدين يتابع الكونتينر من الميناء لحد ما يوصل، وتجيله إشعارات مع كل تحديث. المشروع ده هو "العقل" اللي ورا الموقع أو التطبيق.
