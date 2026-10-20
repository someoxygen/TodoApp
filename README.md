# TodoApp

Vue 3 ve ASP.NET Core ile geliştirilen bir görev yönetimi uygulaması. Kullanıcılar hesap oluşturabilir, giriş yapabilir ve görevleri tek bir ekran üzerinden yönetebilir.

## Özellikler

- Kullanıcı kaydı ve giriş
- Görev listeleme, ekleme, düzenleme ve silme
- Görevlerin aktiflik durumunu yönetme

## Kullanılan teknolojiler

| Frontend | Backend | Veritabanı |
| --- | --- | --- |
| Vue 3, TypeScript, Vite, Axios | ASP.NET Core, Entity Framework Core | PostgreSQL |

## Proje yapısı

- `frontend/todo-frontend`: Kullanıcı arayüzü
- `backend/TodoAPI`: Web API ve veri erişim katmanı

## Çalıştırma

.NET 10 SDK, proje ile uyumlu Node.js sürümü ve PostgreSQL gereklidir. Yerel ortam ayarlarını tamamlayıp veritabanı migration'larını uyguladıktan sonra frontend ve backend'i ayrı terminallerde başlatın.

**Backend** — `backend/TodoAPI/TodoAPI` dizininde:

```sh
dotnet run
```

**Frontend** — `frontend/todo-frontend` dizininde:

```sh
npm ci
npm run dev
```
