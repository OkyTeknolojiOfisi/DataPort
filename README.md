# DataPort API
DataPort'a ait API'ı kullanmak için .NET sınıf kitaplığıdır.

## Örnekler

### SMS Gönderimi

```
using DataPort;
```

```
sms ileti = new sms("KULLANICI", "SIFRE", "SCOPE", sms.OperatorTypes.Turkcell, "ORGINATOR", "SHORTNUMBER", sms.UnicodeTypes.turkce);
ileti.AliciEkle("5XXXXXXXXX", "Deneme mesajı.");
ileti.SMSGonder();
```

### İYS Kaydı Oluşturma

```
using DataPort;
```

```
iys kayit = new iys("KULLANICI", "SIFRE", "BRANDNAME");
kayit.AliciEkle("5XXXXXXXXX", iys.SourceTypes.web);
kayit.KayitGonder();
```
