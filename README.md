# TodoApp

Vue 3 ve ASP.NET Core ile geliştirilen bir görev yönetimi uygulaması. Kullanıcılar hesap oluşturabilir, giriş yapabilir ve görevleri tek bir ekran üzerinden yönetebilir.

## Canlı Demo

[TodoApp'i hemen dene →](https://todoapp-frontend-3irt.onrender.com/)

Kurulum yapmadan tarayıcından hesap oluşturabilir, günlük görevlerini planlayabilir ve kendi görev listeni yönetebilirsin.

> İlk açılışta sunucunun hazırlanması yaklaşık bir dakika sürebilir. Bağlantı kurulduğunda uygulama otomatik olarak açılır.

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

## Kullanıcıya özel görevler

Görevlerin sahibi JWT içindeki kullanıcı kimliğinden belirlenir. Listeleme, arama,
detay, güncelleme ve silme işlemleri yalnızca bu kullanıcının görevlerine erişir.
Farklı kullanıcılar aynı görev başlığını kullanabilir.

`backend/TodoAPI/TodoAPI` dizininde yeni veritabanı migration'ını uygulayın ve
backend'i yeniden başlatın:

```sh
dotnet ef database update --configuration Release
```

Migration, eski görevleri silmez. Önceden sahip bilgisi tutulmadığından bu
kayıtların `UserId` alanı boş kalır ve hiçbir kullanıcının listesinde görünmez.
Bu kayıtlar, sahipleri belirlendikten sonra veritabanında ilgili kullanıcıya
atanabilir. Yeni görevlerde sahip bilgisi API tarafından otomatik atanır.

Kayıt isteği artık `confirmPassword` alanını da gerektirir. Şifreler hem kayıt
formunda hem API'de karşılaştırılır; uyuşmayan veya eksik tekrar alanı olan API
istekleri `400` döner.

## Doğrulama

Proje kök dizininde:

```sh
dotnet test backend/TodoAPI/TodoAPI.slnx --configuration Release
```

API testleri ayrı bir bellek içi SQLite veritabanında gerçek JWT doğrulamasıyla
çalışır; kullanıcılar arası erişimi, sahip bilgisinin istekle değiştirilememesini,
sahipsiz eski görevleri ve şifre tekrarı doğrulamasını denetler. PostgreSQL'e
özel arama ve migration uygulaması bu testlerin kapsamı dışındadır.

Frontend için `frontend/todo-frontend` dizininde `npm run build` komutunu çalıştırın.
