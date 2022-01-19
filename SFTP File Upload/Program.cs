using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.IO;
using System.Net.Sockets;

namespace SFTP_File_Upload
{
    class Program
    {
        const string host = "10.14.1.201"; //cp4
        const int port = 22;
        const string username = "fileTestUser";
        const string password = "fileTestPassword";
        const string workingdirectory = "/user/sharptestfile";
        //const string uploadfile = @"c:\Files\testfile.txt";

        static void Main(string[] args)
        {
            //files dropped onto this exe are imported as args...args[0] in partcular
            if (!(args.Length == 0))
            {
                string uploadfile = Path.GetFullPath(args[0]);                      //get the full path of the file dropped
                Console.WriteLine("Dropped file path: {0}", uploadfile);

                try
                {
                    var client = new SftpClient(host, port, username, password);    //create an sftpclient

                    Console.WriteLine("Creating client and connecting");
                    client.Connect();                                               //connect client

                    if (client.IsConnected)
                    {
                        Console.WriteLine("Connected to {0}", host);

                        try
                        {
                            client.ChangeDirectory(workingdirectory);
                            Console.WriteLine("Changed directory to {0}", workingdirectory);
                        }
                        catch (SftpPathNotFoundException)
                        {
                            client.CreateDirectory(workingdirectory);
                            Console.WriteLine("Directory '{0}' does not exist. Creating...", workingdirectory);
                            Console.WriteLine("Changed directory to {0}", workingdirectory);
                            client.ChangeDirectory(workingdirectory);
                        }

                        using (var fileStream = new FileStream(uploadfile, FileMode.Open))
                        {
                            Console.WriteLine("Uploading {0} ({1:N0} bytes)", uploadfile, fileStream.Length);
                            client.BufferSize = 4 * 1024; // bypass Payload error large files
                            client.UploadFile(fileStream, Path.GetFileName(uploadfile));
                        }
                        client.Disconnect();
                        Console.WriteLine("Disconnecting client.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error creating SftpClient: {0}", e.Message);
                }

                Console.Read();
            }
            
        }
    }
}
