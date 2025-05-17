## Servisler Arası Mesaj Güvenliği Nedir?

İki servisin birbirleriyle mesajlaştığı durumlarda, mesajı gönderen servisin mesajın gerçekten gönderilip gönderilmediğini kontrol etmesi gerekir.

Servisler arasında iletişim şu araçlarla sağlanabilir:

- gRPC
- HTTPS
- Message broker

Hangi yöntem kullanılırsa kullanılsın, mesajın diğer servis tarafından alındığından emin olunmalıdır. Aksi halde tutarsızlıklar meydana gelebilir.

Servisler arası mesaj güvenliği, dağıtık sistemlerde servisler arasında iletilen mesajların güvenli bir şekilde aktarılmasını ifade eder. Mesajın gönderildiğinden emin olmak gönderenin; işlendiğinden emin olmak ise alıcı servisin sorumluluğundadır.

### Senaryo

- Bir servisten farklı bir servise mesaj göndermeden önce, bu mesajı veritabanında bir tabloya fiziksel olarak kaydediyoruz. Bu sayede mesajın **kalıcılığı (durability)** sağlanmış olur.
- Bu tablodaki her bir kayıt için işlem yaparak mesajları hedef servise gönderecek bir uygulama devreye girer. Bu uygulama sayesinde mesajlar garantili bir şekilde hedef servise iletilir ve işlenir.
- Mesaj gönderildikten sonra, tabloda "işlendi" olarak işaretleyebilir veya mesajı tablodan silebiliriz.

Sonuç olarak, gönderici açısından mesajın güvenliğini sağlayan bu tasarım **Outbox Design Pattern** olarak adlandırılır. Bu yapıda mesajların tutulduğu tabloya **Outbox Table**, mesajları hedef servise göndermekle sorumlu servise ise **Outbox Publisher Application** denir.

### Senaryo Devamı

- Outbox Publisher’dan çıkan mesaj, alıcı servisin **Inbox Table**’ına yazılır. Daha sonra alıcı servis bu mesajları Inbox Table’dan tüketir.

Bu senaryoda, alıcı açısından mesajın güvenliğini sağlayan tasarım ise **Inbox Design Pattern** olarak adlandırılır.

---

### Özet

Asenkron iletişim kuran sistemlerde, servisler arası (ya da servis ile message broker arası) iletişim süreçlerinde, gönderilen mesajların hedefe ulaşmadan bağlantının kopması, hata alınması veya beklenmedik fiziksel/yazılımsal kesintiler gibi durumlar yaşanabilir.

Bu tür durumlar, veri kayıplarına ve servisler arası tutarsızlıklara yol açabilir. Outbox/Inbox tasarım desenleri, mesajın kaybolmasını engeller ve bağlantı tekrar sağlandığında mesajın yeniden gönderilmesini mümkün kılar. Basit ama kritik öneme sahip bu desenler, dağıtık sistemlerde güvenilirliği artırır.

---

## Outbox Design Pattern Hangi Durumlarda Kullanılır?

Outbox Pattern, bir servis mesaj veya event yayınladığında, doğrudan hedef servise ya da message broker'a göndermek yerine bu mesajları bir veritabanı tablosuna kaydeder. Daha sonra bir publisher uygulaması aracılığıyla belirli aralıklarla bu mesajlar hedefe iletilir.

Bu yapı sayesinde, mesajlara **en az bir kere gönderim garantisi (at-least-once delivery)** sağlanmış olur.

### Örnek Kullanım Durumları

- Sipariş verildikten sonra kullanıcıya e-posta göndermek
- Kullanıcı kaydı hakkında message broker’a event göndermek
- Sipariş sonrası stoktaki ürün sayısını güncellemek

---

## Inbox Design Pattern

Inbox Pattern’de, alıcı servise publisher aracılığıyla gelen mesajlar önce Inbox Table’a yazılır, ardından bu mesajlar işlenir.

---

## Idempotent Sorunsalı

Idempotent, bir mesaj birden fazla kez yayınlansa dahi, alıcı servis açısından aynı etkiyi üretmesi anlamına gelir.

Mesaj, Outbox tablosunda "gönderildi" olarak güncellenmeden önce alıcıya iletilirse, aynı mesajın tekrar işlenmemesi gerekir. Bu problemi çözmek için her mesaja **benzersiz bir ID (idempotent token)** verilir.

Inbox tablosunda bu token varsa, mesaj işleme alınmaz. Bu yöntem sayesinde mesajların tekrar işlenmesi engellenmiş olur.

Idempotent nasıl sağlanır;

- Unique id vererek.
- Atomik işlem yaparak, işlemin tüm adımlarının başarılı bir şekilde tamamlanmasını yahut komple işlemin iptal edilmesini sağlayabiliriz. Bu davranışla, işlemlerin tekrar etmesine sebep olan olası durumların yaşanma ihtimalini sıfıra indirgeyebilirz. Örneğin, strong consistency 2Phase Commit kullanarak.
- İşlem sonuçlarını önbelleğe alarak, aynı işlemin tekrar çağrılması durumunda önceki sonuçları hızlı bir şekilde döndürebilir ya da baz alabiliriz.
- Zaman damgası (Timestamp): Her işleme/mesaja isteğe zaman damgası ekleyebilir ve böylece belirli bir zaman aralığında aynı işlemin tekrar edilip edilmediğini öğrenebiliriz.

Not: Mikroservis mimarilerde genelde unique id yöntemi kullanılır.

---

## Özetle

- Bir servis kendi business'larını yaptıktan sonra fırlatacağı event sınıfını oluşturur. Outbox table'a payload kısmına serialize edilecek şekilde kayıt atar.
- Outbox publisher'da worker görevi görür belli aralıklarla tablosunda işlenmemiş kayıt var mı kontol eder, gelen kaydı da ilgili servise publish eder.
- Alıcı servis mesajı yakalayınca inbox tablosuna eğer aynı idempotent token ile gelinmemişse kaydeder. Daha sonrasında ise sırasıyla business'ini koşar.

## Kaynakça

- Gençay Yıldız - Mikroservis Eğitimleri (YouTube)

---

## Proje

- Order.API servisi, /create-order ile db'ye yeni bir order ekler.
- Db order eklendikten sonra event fırlatır. OrderCreateEvent türünde.
- Bu durumda hem db yazma hem event fırlatma iki ayrı olay olduğu için tutarsızlık durumu ele alınmalı.
- Çözüm olarak order.API servisinde, oluşturulan order'ların publish edilecek halleri OrderOutboxes tablosuna yazılır. (Payload kısmında event sınıfındaki veriler serialize edilir)
- Order.Outbox.Table.Publisher.Service worker servis belli aralıklarla çalışarak OrderOutboxes tablosundan veriyi stock servise publish yapar. Daha sonra outbox tablosunda ProccessedDate alanını günceller.
- Mesajı consume eden Stock.Service, Idempotent kontrolünden (aynı mesaj gitmemesi için) sonra OrderInboxes tablosuna insert atar. Processed 0 olarak insert atar. Sonrasında ise işlemeye başlar. İşlediklerinin processed'ini 1'e çeker.

- Configler;
- RabbitMQ: docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:4.0-management

- Mssql Sorgular

SELECT top 5 FROM OrderDB.dbo.Orders nolock order by 1 desc

SELECT top 5 from OrderDB.dbo.OrderItems nolock order by 1 desc

SELECT top 5 FROM OrderDB.dbo.OrderOutboxes nolock order by 1 desc

SELECT top 5 from StockDb.dbo.OrderInboxes nolock
