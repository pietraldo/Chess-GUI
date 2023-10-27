using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Szachy3
{
    internal class Board
    {
        public Figure[,] plansza = new Figure[8,8];
        public Board(Game1.Piece[] figury)
        {
            foreach(Game1.Piece figura in figury)
            {
                if (figura.zbity) continue;
                Kolor_figury k = (figura.picture.Name[0]=='w')?Kolor_figury.WHITE: Kolor_figury.BLACK;
                switch(figura.picture.Name[1])
                {
                    case 'B':
                        plansza[7-(int)(figura.position.Y / 100), (int)(figura.position.X / 100)] = new Bishop(k);
                        break;
                    case 'R':
                        plansza[7 - (int)(figura.position.Y / 100), (int)(figura.position.X / 100)] = new Rook(k);
                        break;
                    case 'N':
                        plansza[7 - (int)(figura.position.Y / 100), (int)(figura.position.X / 100)] = new Knight(k);
                        break;
                    case 'P':
                        plansza[7 - (int)(figura.position.Y / 100), (int)(figura.position.X / 100)] = new Pawn(k);
                        break;
                    case 'K':
                        plansza[7 - (int)(figura.position.Y / 100), (int)(figura.position.X / 100)] = new King(k);
                        break;
                    case 'Q':
                        plansza[7 - (int)(figura.position.Y / 100), (int)(figura.position.X / 100)] = new Queen(k);
                        break;
                }
               
            }
        }

        public void Show_Board()
        {
            for(int i=7; i>=0 ; i--)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (plansza[i, j] != null)
                        Debug.Write(plansza[i,j].Show()+" ");
                    else
                        Debug.Write(". ");
                }
                Debug.WriteLine("");
            }
        }
    }
    internal class Move
    {
        public int i1;
        public int j1;
        public int i2;
        public int j2;
        public Move(Vector2 a, Vector2 b)
        {
            i1 = 7 - (int)(a.Y / 100);
            j1 = (int)(a.X / 100);
            i2 = 7 - (int)(b.Y / 100);
            j2 = (int)(b.X / 100);
        }
        public Move(int i1, int j1, int i2, int j2)
        {
            this.i1 = i1;
            this.j2 = j2;
            this.j1 = j1;
            this.i2 = i2;
        }
        public Vector2 Odwrotnie_Koniec()
        {
            return new Vector2(j2*100, (7-i2)*100);
        }
        public static int[] VectorToInt(Vector2 b)
        {
            int[] a = new int[2];
            a[0] = 7 - (int)(b.Y / 100);
            a[1] = (int)(b.X / 100);
            return a;
        }
        public void Show()
        {
            Debug.WriteLine("("+i1+", "+j1+") ->"+ "(" + i2 + ", " + j2+")");
        }
    }
    internal abstract class Figure
    {
        public Kolor_figury kolor;
        protected Figure(Kolor_figury kolor) 
        {
            this.kolor= kolor;
        }
        public abstract int GetPoints();
        public abstract void Moves(Board b, int i, int j, List<Move> ruchy);
        public abstract char Show();
    }

    internal class Rook: Figure
    {
        public Rook(Kolor_figury kolor):base(kolor){}
        public override int GetPoints() { return 5; }
        public  override void Moves(Board b, int i, int j, List<Move> ruchy)
        {
            
            for(int k=j+1; k < 8 && b.plansza[i,k] == null ; k++)
            {
                ruchy.Add(new Move(i,j,i,k));
            }
            for (int k = j-1; k >= 0 && b.plansza[i, k] == null; k--)
            {
                ruchy.Add(new Move(i, j, i, k));
            }
            for (int k = i+1; k < 8 && b.plansza[k,j] == null; k++)
            {
                ruchy.Add(new Move(i, j, k, j));
            }
            for (int k = i-1; k >= 0 && b.plansza[k, j]==null; k--)
            {
                ruchy.Add(new Move(i, j, k, j));
            }
        }
        public override char Show()
        {
            return (kolor == Kolor_figury.WHITE) ? 'R' : 'r';
        }
    }
    internal class Queen : Figure
    {
        public Queen(Kolor_figury kolor) : base(kolor) { }
        public override int GetPoints() { return 9; }
        public override void Moves(Board b, int i, int j, List<Move> ruchy)
        {
            
        }
        public override char Show()
        {
            return (kolor == Kolor_figury.WHITE) ? 'Q' : 'q';
        }
    }
    internal class Bishop : Figure
    {
        public Bishop(Kolor_figury kolor) : base(kolor) { }
        public override int GetPoints() { return 3; }
        public override void Moves(Board b, int i, int j, List<Move> ruchy)
        {
            
        }
        public override char Show()
        {
            return (kolor == Kolor_figury.WHITE) ? 'B' : 'b';
        }
    }
    internal class Knight : Figure
    {
        public Knight(Kolor_figury kolor) : base(kolor) { }
        public override int GetPoints() { return 3; }
        public override void Moves(Board b, int i, int j, List<Move> ruchy)
        {

        }
        public override char Show()
        {
            return (kolor == Kolor_figury.WHITE) ? 'N' : 'n';
        }
    }
    internal class King : Figure
    {
        public King(Kolor_figury kolor) : base(kolor) { }
        public override int GetPoints() { return 10000; }
        public override void Moves(Board b, int i, int j, List<Move> ruchy)
        {

        }
        public override char Show()
        {
            return (kolor == Kolor_figury.WHITE) ? 'K' : 'k';
        }
    }
    internal class Pawn : Figure
    {
        public Pawn(Kolor_figury kolor) : base(kolor) { }
        public override int GetPoints() { return 1; }
        public override void Moves(Board b, int i, int j, List<Move> ruchy)
        {

        }
        public override char Show()
        {
            return (kolor == Kolor_figury.WHITE) ? 'P' : 'p';
        }
    }
}
