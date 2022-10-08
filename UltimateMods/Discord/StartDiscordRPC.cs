// using System.Diagnostics;
// using System.IO;

// namespace UltimateMods
// {
//     public static class DiscordPatch
//     {
//         static StreamWriter writer;

//         public static void StartDiscord()
//         {
//             Process p = new Process();
//             p.OutputDataReceived += new DataReceivedEventHandler(DataReceived);

//             p.StartInfo.FileName = "python";
//             p.StartInfo.Arguments = "DiscordRPC.py";
//             p.StartInfo.UseShellExecute = false;
//             p.StartInfo.RedirectStandardOutput = true;
//             p.StartInfo.RedirectStandardInput = true;
//             p.StartInfo.CreateNoWindow = true;
//             p.Start();
//             p.BeginOutputReadLine();

//             writer = p.StandardInput;
//             writer.AutoFlush = true;
//         }
//         static void DataReceived(object sender, DataReceivedEventArgs e)
//         {
//             Helpers.Log(e.Data);
//         }
//     }
// }