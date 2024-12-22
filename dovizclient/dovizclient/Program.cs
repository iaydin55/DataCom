using System;
using System.Net.Sockets;
using System.Text;

class Client
{
    static void Main()
    {
        try
        {
            // Server'a bağlan
            TcpClient client = new TcpClient("127.0.0.1", 5000);
            Console.WriteLine("Döviz sunucusuna bağlanıldı!");

            NetworkStream stream = client.GetStream();

            while (true)
            {
                // Kullanıcıdan döviz türü al
                Console.Write("Döviz bilgisi almak istediğiniz para birimini girin (örnek: USD): ");
                string currency = Console.ReadLine().ToUpper();

                if (string.IsNullOrEmpty(currency))
                    break;

                // Talebi server'a gönder
                byte[] data = Encoding.UTF8.GetBytes(currency);
                stream.Write(data, 0, data.Length);

                // Server'dan dönen cevabı oku
                byte[] buffer = new byte[1024];
                int byteCount = stream.Read(buffer, 0, buffer.Length);
                string response = Encoding.UTF8.GetString(buffer, 0, byteCount);

                Console.WriteLine("Sunucudan gelen cevap: " + response);
            }

            client.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Hata: " + ex.Message);
        }
    }
}