using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.Json;

class Server
{
    static void Main()
    {
        // TCP Listener başlatılır
        TcpListener server = new TcpListener(IPAddress.Any, 5000);
        server.Start();
        Console.WriteLine("Döviz sunucusu başlatıldı. İstemciler bekleniyor...");

        while (true)
        {
            // Yeni istemci bağlantısını kabul et
            TcpClient client = server.AcceptTcpClient();
            Console.WriteLine("Yeni bir istemci bağlandı!");

            // İstemciyi ayrı bir thread'de işleme al
            Thread clientThread = new Thread(HandleClient);
            clientThread.Start(client);
        }
    }

    static void HandleClient(object obj)
    {
        TcpClient client = (TcpClient)obj;
        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[1024];
        int byteCount;

        try
        {
            while ((byteCount = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, byteCount);
                Console.WriteLine("İstemciden gelen talep: " + receivedMessage);

                // Döviz API'sinden döviz bilgisi al
                string responseMessage = GetExchangeRate(receivedMessage);

                // Yanıtı istemciye gönder
                byte[] response = Encoding.UTF8.GetBytes(responseMessage);
                stream.Write(response, 0, response.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Hata: " + ex.Message);
        }
        finally
        {
            client.Close();
            Console.WriteLine("İstemci bağlantısı kapatıldı.");
        }
    }

    // Döviz bilgisi almak için API'ye istek at
    static string GetExchangeRate(string currency)
    {
        try
        {
            // API Key ve URL (base parametresi olmadan)
            string apiKey = "5750d3388f55338b9ac31f1a";
            //string apiUrl = $"https://https://www.tldoviz.com/apikey={apiKey}";
            string apiUrl = $"https://v6.exchangerate-api.com/v6/5750d3388f55338b9ac31f1a/latest/{currency}";
            Console.WriteLine("API'ye gönderilen URL: " + apiUrl);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(apiUrl);
            request.Method = "GET";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string jsonResponse = reader.ReadToEnd();
                    Console.WriteLine("API'den dönen yanıt: " + jsonResponse);

                    // JSON yanıtını işle
                    JsonDocument json = JsonDocument.Parse(jsonResponse);
                    if (json.RootElement.TryGetProperty("conversion_rates", out var rates))
                    {
                        if (rates.TryGetProperty("TRY", out var tryRate))
                        {
                            return $"1 {currency} = {tryRate.GetDouble()} TRY";
                        }
                        else
                        {
                            return "Hata: TRY kuru bulunamadı.";
                        }
                    }
                    else
                    {
                        return "Hata: Döviz bilgisi alınamadı.";
                    }

                    //if (!json.RootElement.TryGetProperty("rates", out var rates))
                    //{
                    //    return "Hata: Döviz bilgisi alınamadı.";
                    //}

                    //// Kullanıcının istediği döviz oranını hesapla
                    //if (rates.TryGetProperty(currency, out var currencyRate))
                    //{
                    //    return $"1 EUR = {currencyRate.GetDouble()} {currency}";
                    //}
                    //else
                    //{
                    //    return $"Hata: {currency} için döviz oranı bulunamadı.";
                    //}
                }
            }
        }
        catch (WebException webEx)
        {
            using (var errorResponse = (HttpWebResponse)webEx.Response)
            {
                using (var reader = new StreamReader(errorResponse.GetResponseStream()))
                {
                    string error = reader.ReadToEnd();
                    return "Hata: " + error;
                }
            }
        }
        catch (Exception ex)
        {
            return "Hata: " + ex.Message;
        }
    }




}