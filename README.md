# Restoran QR Menü Sistemi

Bu proje, ASP.NET Core 8.0 MVC ve Entity Framework Core kullanılarak geliştirilmiş bir dijital restoran menü sistemidir.

## Özellikler

- **Müşteri Arayüzü (QR Menü)**: Masalardaki QR kod okutularak erişilen, mobil uyumlu dijital menü.
- **Sepet Sistemi**: Müşterilerin istedikleri ürünleri sepete ekleyip "Siparişi Tamamla" diyerek sipariş verebilmesi (Session tabanlı).
- **Yönetici (Admin) Paneli**:
  - **Dashboard**: Chart.js kullanılarak en çok tercih edilen ürünlerin grafiksel gösterimi.
  - **Ürün Yönetimi**: Ürünlerin listelenmesi.
  - **Raporlama**: Ürün listesinin "Excel Olarak İndir" ve "PDF Olarak İndir" butonları yardımıyla dışa aktarılması (ClosedXML ve iText7 ile).
- **Dark Mode (Gece Modu)**: JavaScript ve CSS Değişkenleri (Variables) kullanılarak yapılmış, kullanıcının tercihini `localStorage`'da saklayan gece/gündüz modu düğmesi.
- **Veritabanı**: SQL Server (LocalDB) ve Entity Framework Core Code-First yaklaşımı.

## Kurulum ve Çalıştırma

1. Projeyi Visual Studio 2022 ile açın.
2. `appsettings.json` içerisindeki `DefaultConnection` bağlantı dizesini kendi SQL Server ayarlarınıza göre (gerekirse) güncelleyin. Varsayılan olarak `(localdb)\mssqllocaldb` kullanmaktadır.
3. Paket Yöneticisi Konsolunda (Package Manager Console) aşağıdaki komutu çalıştırarak veritabanını oluşturun:
   ```bash
   Update-Database
   ```
4. Projeyi çalıştırın (`F5` veya `Ctrl+F5`).
5. İlk çalıştırmada örnek veriler (Çorbalar, Kebaplar, İçecekler ve bu kategorilere ait ürünler) veritabanına otomatik eklenecektir (Eğer Program.cs içerisinde seed metodu ayarlandıysa).

## Sayfalar

- **Müşteri Menüsü**: `https://localhost:PORT/`
- **Sepet**: `https://localhost:PORT/Cart`
- **Admin Paneli**: `https://localhost:PORT/Admin`

## Ekran Görüntüleri İçin

*Proje teslimi sırasında bu bölüme uygulamanın çalışan halinden alınan ekran görüntüleri eklenebilir.*

- **Müşteri Arayüzü & Sepet**
- **Dark Mode Görünümü**
- **Admin Dashboard (Chart.js)**
- **Excel/PDF İndirme Butonları**

## Kullanılan Teknolojiler

- .NET 8.0
- ASP.NET Core MVC
- Entity Framework Core 8.0 (SQL Server)
- Bootstrap 5
- Chart.js
- ClosedXML (Excel dışa aktarımı)
- iText7 (PDF dışa aktarımı)
