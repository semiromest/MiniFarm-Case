# MiniFarm-Case

Merhabalar!

MiniFarm Case görevini başarılı bir şekilde bitirdiğimi düşünüyorum. Bu sistemi geliştirirken birkaç temel prensibe odaklandım:

1. Reactive Programming (UniRx) Kullanımı:
UniRx kütüphanesini kullanarak, oyun içindeki durum değişikliklerini (örneğin, kaynak miktarı, üretim süresi, kuyruk durumu) reactive 
property'ler ve reactive collection'lar ile yönettim. Bu sayede, UI elemanları ve diğer sistemler arasında otomatik senkronizasyon sağlandı.

Örneğin, _currentResourceAmount değiştiğinde, UI otomatik olarak güncelleniyor ve butonların etkileşim durumu yeniden hesaplanıyor.

2. Üretim Kuyruğu ve Kaynak Yönetimi:
Üretim kuyruğu (_productionQueue) ReactiveCollection ile yönetiliyor. Bu sayede, kuyruğa yeni üretim emri eklendiğinde veya kuyruktan 
bir emir çıkarıldığında, otomatik olarak UI güncelleniyor ve gerekli kaynaklar harcanıyor veya geri veriliyor.

Kaynak yönetimi, GameManager üzerinden yapılıyor. Üretim emri eklenirken kaynaklar kontrol ediliyor ve yeterli kaynak yoksa işlem gerçekleşmiyor.

3. Offline Üretim Simülasyonu:
Oyun kapatıldığında veya oyuncu oyundan çıktığında, üretim süreci devam ediyor. SaveGameData ve LoadGameData metodları ile oyun verileri 
kaydediliyor ve yüklendiğinde, geçen süre hesaplanarak offline üretim simüle ediliyor.

Bu sayede, oyuncu oyuna geri döndüğünde, üretim süreci kaldığı yerden devam ediyor ve kaynaklar otomatik olarak toplanıyor.

4. UI Etkileşimi ve Kullanıcı Deneyimi:
Üretim paneli, fabrikaya tıklandığında açılıyor ve UI dışında bir yere tıklandığında kapanıyor. Bu, kullanıcı deneyimini artırmak için önemli bir detay.

Butonların etkileşim durumu, mevcut kaynak miktarına ve kuyruk durumuna göre dinamik olarak güncelleniyor. Örneğin, yeterli kaynak yoksa veya kuyruk 
doluysa, butonlar devre dışı bırakılıyor.

5. Genişletilebilirlik ve Soyutlama:
BaseProductionFacility sınıfı, soyut bir yapıda tasarlandı. Bu sayede, farklı üretim tesisleri (örneğin, buğday üretimi, un üretimi) 
bu sınıftan türetilerek kolayca genişletilebilir.

ProductionTime, MaxCapacity, ResourcePerProduction, ResourceKey gibi özellikler, türetilen sınıflarda override edilerek özelleştirilebilir.

Kayıt sistemini PlayerPrefsler ile yaptım ancak öncesinde json denedim ve bir türlü istediğim sonucu alamadım. Bu yüzden Playerprefse geri döndüm daha fazla 
zamanım olsaydı jsonla ya da daha güvenli bir yöntem ile yapardım.

Case modüler ve scale edilebilir olması gerektiği için önce genel bir sınıf ürettim ve diğer fabrikaları ondan türettim. Hayfactory biraz farklı olduğu için 
zorladı ancak override ederek çözdüm. Yeni bir fabrika üretilmek istenirse basitçe BaseProductionFacility sınıfından türetmek ve değerleri girmek yeterli olacaktır.

Zenject ve UniTask başlangıçta kullanmak istedim ancak iyi bir mimari kullandığım için bağımlılıklar azaldı ve onları kullanmanın performansa çokta etki etmeyeceklerini düşündüm.
Sadece UniRX kullandım.

Umarım bu proje Case'i başarılı bir şekilde tamamlamış olmam için yeterlidir. İyi günler diliyorum.

_İBRAHİM MELİH DOĞAN
