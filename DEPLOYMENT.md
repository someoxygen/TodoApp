# Render + Neon ile yayınlama

Bu kurulumda Vue arayüzü Render Static Site, .NET 10 API Render Free Web Service,
PostgreSQL ise Neon Free üzerinde çalışır. Render'ın verdiği HTTPS adreslerini
kullanabilirsin; alan adı satın alman gerekmez.

## 1. Kodu GitHub'a gönder

Bu değişiklikleri commit edip GitHub repona push et. Repo private olabilir;
Render'a ilgili repoya erişim izni ver. `appsettings.json` ve `.env` yerel
dosyalardır, Git'e ekleme. `.dockerignore` da yerel ayarları imajdan dışlar.

## 2. Neon veritabanını hazırla

[Neon](https://console.neon.tech/) hesabında Free planla bir proje oluştur.
Mümkünse API için seçeceğin Render bölgesine yakın bir bölge seç.
Yerelde zaten Neon kullanıyorsan mevcut veritabanını kullanabilirsin; geliştirme
verilerinin yayındaki uygulamada görünmemesi için ayrı proje tercih edebilirsin.
Yeni veritabanına geçmek mevcut verileri taşımaz, yalnızca tablolar oluşturulur.

Connect ekranından host, database, role/username ve password bilgilerini al.
API için aşağıdaki Npgsql biçiminde tek satır hazırla:

```text
Host=NEON_HOST;Database=NEON_DATABASE;Username=NEON_ROLE;Password=NEON_PASSWORD;SSL Mode=VerifyFull;Channel Binding=Require;
```

Yer tutucuları kendi değerlerinle değiştir. `postgresql://...` URL'sini doğrudan
bu ayara yapıştırma. Migration da aynı bağlantıyı kullanacağından bu tek API
kurulumunda Neon'un doğrudan bağlantı hostunu (pooling kapalı) kullan.

## 3. Render'da API oluştur

[Render Dashboard](https://dashboard.render.com/) → New → Web Service → GitHub repon.

| Alan | Değer |
| --- | --- |
| Name | İstediğin benzersiz API adı |
| Language / Runtime | Docker |
| Root Directory | `backend/TodoAPI/TodoAPI` |
| Dockerfile Path | `./Dockerfile` (root directory'ye göre) |
| Docker Build Context Directory | `.` (alan görünüyorsa) |
| Instance Type | **Free** |
| Health Check Path | `/health` |

Docker Command alanını boş bırak. Dockerfile API'yi 10000 portunda çalıştırır.
Environment bölümüne aşağıdaki değişkenleri ekle (çift alt çizgilere dikkat):

| Key | Value |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | 2. adımdaki Npgsql bağlantı metni |
| `Jwt__Key` | Yeni, rastgele bir gizli anahtar |
| `Jwt__Issuer` | `Todo.API` |
| `Jwt__Audience` | `Todo.Client` |
| `Jwt__DurationInMinutes` | `60` |
| `Database__ApplyMigrations` | `true` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ASPNETCORE_HTTP_PORTS` | `10000` |

JWT anahtarını PowerShell'de üretip yalnızca Render'daki `Jwt__Key` alanına koy:

```powershell
$jwtBytes = New-Object byte[] 32
$jwtRng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$jwtRng.GetBytes($jwtBytes)
[Convert]::ToBase64String($jwtBytes)
$jwtRng.Dispose()
```

Deploy et. API başlangıçta bekleyen EF Core migration'larını uygular; bağlantı
hatalıysa yayın başarısız olur. Bu ayar tek instance'lı küçük kurulum içindir.
İleride veritabanı şemasını değiştiren yayınlardan önce yedek al.

Render'ın verdiği gerçek API adresini kaydet. Örneğin
`https://senin-api-adin.onrender.com/health` adresi `{"status":"ok"}` döndürmeli.
Bu kontrol API'nin çalıştığını gösterir, sürekli veritabanı kontrolü yapmaz.
API ana sayfasında `/` için 404 normaldir; Swagger Production'da kapalıdır.

## 4. Render'da frontend oluştur

New → **Static Site** → aynı GitHub reposunu seç.

| Alan | Değer |
| --- | --- |
| Root Directory | `frontend/todo-frontend` |
| Build Command | `npm ci && npm run build` |
| Publish Directory | `dist` |
| Environment: `NODE_VERSION` | `24.20.0` |
| Environment: `VITE_API_URL` | `https://GERCEK-API-ADRESIN.onrender.com/api` |

API adresinin sonundaki `/api` gereklidir. Frontend'e veritabanı parolası veya
JWT anahtarı ekleme: `VITE_` değişkenleri ziyaretçilerin tarayıcısına gönderilir.
Deploy et ve oluşan frontend HTTPS adresini kaydet.

Static Site → Redirects/Rewrites bölümüne şunu ekle:

| Source | Destination | Action |
| --- | --- | --- |
| `/*` | `/index.html` | Rewrite |

Bu kural `/login`, `/register` ve `/todos` sayfalarını yenilerken 404 alınmasını önler.

## 5. API'nin frontend'e izin vermesini sağla

API Web Service → Environment bölümüne ekle:

```text
Cors__AllowedOrigins__0=https://GERCEK-FRONTEND-ADRESIN.onrender.com
```

Değerde `/api`, `/login` veya sonda `/` bulunmasın. Save, rebuild, and deploy
seçeneğiyle API'yi yeniden yayınla. Frontend adresi oluşana kadar kayıt/giriş
isteklerinin CORS nedeniyle çalışmaması normaldir.

## 6. Yayını kontrol et

- Frontend adresinden yeni hesap oluştur, giriş yap, görev ekle/düzenle/sil.
- Sayfayı yenile ve kayıtların korunduğunu doğrula.
- `/login` bağlantısını doğrudan yeni sekmede aç.
- İkinci hesapta ilk hesabın görevlerinin görünmediğini kontrol et.

Render API 15 dakika trafik almayınca uyur; sonraki ilk istek yaklaşık bir
dakika gecikebilir. Neon Free işlem/depolama kotaları da geçerlidir. Free planları
seç; Render Postgres oluşturma, onun ücretsiz veritabanı 30 gün sonra sona erer.

GitHub'a sonraki push'larda bağlı dal için otomatik yayın açıksa Render yeniden
build alır. `VITE_API_URL` değişirse frontend'i tekrar build/deploy et;
frontend adresi değişirse API'nin CORS ayarını güncelle.

## Kaynaklar

- [Render Docker kurulumu](https://render.com/docs/docker)
- [Render Vue ve SPA yönlendirmeleri](https://render.com/docs/deploy-vue-js)
- [Render Node sürümü](https://render.com/docs/node-version)
- [Render ücretsiz plan sınırları](https://render.com/docs/free)
- [Neon ücretsiz plan](https://neon.com/pricing)
- [Npgsql bağlantı parametreleri](https://www.npgsql.org/doc/connection-string-parameters)
