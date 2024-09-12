using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Szachy3
{
    internal class Bot
    {
        Kolor_figury kolor_bota =Game1.engine_color;
        string engine_path = Game1.engine_path;
        string engine_depth = Game1.engine_depth;

        public Bot() { }
        public Move MakeMove(Board b)
        {
            List<Move> list = new List<Move>();
            for(int i=0; i <8; i++)
            {
                for(int j=0; j<8; j++)
                {
                    if (b.plansza[i,j]!=null && b.plansza[i,j].kolor==kolor_bota)
                    {
                        b.plansza[i, j].Moves(b, i, j, list, true);
                    }
                }
            }

            //Debug.WriteLine("----------Ruchy bota----------");
            //foreach (Move a in list)
            //    a.Show();
            //Debug.WriteLine("----------------------------");
            Random rnd = new Random();
            
            return list[rnd.Next(list.Count)];
            
        }
        
        public string MakeFEN(Board b, string czyj_ruch)
        {
            string fen="";
            for(int i=7; i>=0 ; i--)
            {
                int przerwa = 0;
                for(int j=0; j<8; j++)
                {
                    if (b.plansza[i,j]!=null)
                    {
                        if (przerwa != 0)
                            fen += przerwa.ToString();
                        fen += b.plansza[i, j].Show();
                        przerwa = 0;
                    }
                    else
                    {
                        przerwa++;
                    }
                }
                if(przerwa!=0)
                fen += przerwa.ToString();
                if (i != 0) fen += "/";

            }
            fen +=" "+ czyj_ruch+ " - - 0 1";
            return fen;
        }

        public Move MakeMoveFromFileEngine(Board b)
        {
            string fen = MakeFEN(b, (kolor_bota==Kolor_figury.WHITE)?"w":"b");
            // Define the process start info
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = engine_path,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            string response1 = "";
            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();

                // Set up streams for input and output
                StreamWriter sw = process.StandardInput;
                StreamReader sr = process.StandardOutput;

                // Send a command to the external program
                Debug.WriteLine(fen);
                sw.WriteLine("position fen "+fen);
                sw.WriteLine("go depth "+engine_depth);

                do
                {
                    response1 = sr.ReadLine();
                    Console.WriteLine("Response 1: " + response1);
                }
                while (!response1.Contains("bestmove"));
                // Read the response
                response1 = response1.Split(" ")[1];
                // Close the streams and wait for the process to exit
                sw.Close();
                sr.Close();
                process.WaitForExit();


            }
            return Move.UciToMove(response1);
        }
    }

    

}
