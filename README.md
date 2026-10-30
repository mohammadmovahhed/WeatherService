<div dir="rtl">

# 🌦️ سرویس آب‌وهوای خودروهای خودران (WeatherService)

سرویس WeatherService یک API مبتنی بر NET. است که برای خودروهای خودران طراحی شده. این سرویس اطلاعات آب‌وهوای لحظه‌ای رو بر اساس مختصات جغرافیایی خودرو از یک API خارجی دریافت می‌کنه، داده خام رو در پایگاه داده محلی SQLite ذخیره می‌کنه و همون داده رو بدون تغییر به کلاینت (مثل سیستم تهویه، موتور یا ماژول مدیریت انرژی) برمی‌گردونه.



## ویژگی‌های کلیدی و تحمل خطا

- مختصات پویا: سرویس مختصات `lat` و `lon` رو از کلاینت می‌گیره. اگه ارسال نشه، به صورت پیش‌فرض از مختصات برلین استفاده می‌کنه.
- مقاومت در برابر خطای شبکه: با توجه به اینکه ارتباط بی‌سیم خودرو با اینترنت حدود ۳۰٪ خطا و نویز داره، سرویس طوری طراحی شده که در صورت قطعی اینترنت از کار نیفته.
- پشتیبان‌گیری محلی (Fallback): اگه ارتباط با API خارجی برقرار نشه، سرویس بلافاصله آخرین داده موفق ذخیره‌شده در دیتابیس SQLite رو برمی‌گردونه.
- مدیریت خطای شفاف: اگه اینترنت قطع باشه و دیتابیس هم خالی باشه، مقدار `null` برگردونده می‌شه.
- تضمین زمان پاسخ‌دهی (حداکثر ۳ ثانیه): اگه دریافت داده از اینترنت بیشتر از ۳ ثانیه طول بکشه، سیستم به صورت خودکار لغو می‌کنه و به سراغ دیتابیس می‌ره تا کلاینت پشت خط منتظر نمونه.
- یکپارچگی داده‌ها: داده‌های دریافتی به صورت کاملاً خام (Raw JSON) ذخیره و بدون هیچ تغییری به کلاینت برگردونده می‌شن.

## معماری سیستم

پروژه با استفاده از Minimal API پیاده‌سازی شده:

- Endpoint (`Program.cs`): مدیریت درخواست‌های HTTP و اعمال محدودیت زمانی ۳ ثانیه.
- Orchestrator (`WeatherOrchestrator`): مغز متفکر سیستم که تصمیم می‌گیره داده رو از اینترنت بگیره یا از دیتابیس.
- External Service (`ExternalWeatherService`): ارتباط با API خارجی (Open-Meteo) با استفاده از `HttpClient`.
- Database Service (`DatabaseService`): مدیریت پایگاه داده با استفاده از Entity Framework Core.

## نحوه استفاده از API

### دریافت اطلاعات آب‌وهوا

`GET /api/weather`

پارامترهای ورودی (اختیاری):

- `lat`: عرض جغرافیایی خودرو (مثلاً `35.6892`)
- `lon`: طول جغرافیایی خودرو (مثلاً `51.3890`)

نمونه درخواست:

```http
GET https://localhost:xxxx/api/weather?lat=35.6892&lon=51.3890
```

نمونه خروجی (داده خام JSON):

```json
{
  "latitude": 35.667652,
  "longitude": 51.403595,
  "generationtime_ms": 0.43129920959472656,
  "utc_offset_seconds": 0,
  "timezone": "GMT",
  "timezone_abbreviation": "GMT",
  "elevation": 1173.0,
  "current_weather_units": {
    "time": "iso8601",
    "interval": "seconds",
    "temperature": "°C",
    "windspeed": "km/h",
    "winddirection": "°",
    "is_day": "",
    "weathercode": "wmo code"
  },
  "current_weather": {
    "time": "2026-10-30T22:15",
    "interval": 900,
    "temperature": 15.5,
    "windspeed": 7.7,
    "winddirection": 37,
    "is_day": 0,
    "weathercode": 80
  }
}
```

## نحوه اجرای پروژه

### پیش‌نیازها

- [NET 10 SDK.](https://dotnet.microsoft.com/download)
- Entity Framework Core CLI (`dotnet tool install --global dotnet-ef`)

### مراحل اجرا

۱. دریافت پروژه از گیت‌هاب:

```bash
git clone https://github.com/mohammadmovahhed/WeatherService.git
cd WeatherService
```

۲. ساخت دیتابیس محلی:

```bash
dotnet ef database update
```

۳. اجرای برنامه:

```bash
dotnet run
```


## تکنولوژی‌های استفاده شده

- C# / .NET 10 (LTS)
- ASP.NET Core Minimal API
- Entity Framework Core
- SQLite (دیتابیس محلی با پایداری بالا)
- Open-Meteo API (سرویس رایگان آب‌وهوا بدون نیاز به کلید دسترسی)


</div>