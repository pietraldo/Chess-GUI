using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Szachy3
{
    internal class Board
    {
        public Figure[,] plansza = new Figure[8, 8];
        private List<Move> historia;

        public bool[] roszady_bialych = new bool[] { true, true };
        public bool[] roszady_czarnych = new bool[] { true, true };
        public Board(Game1.Piece[] figury, List<Move> historia = null)
        {
            this.historia = (historia == null) ? new List<Move>() : historia;
            foreach (Game1.Piece figura in figury)
            {
                if (figura.zbity) continue;
                Kolor_figury k = (figura.picture.Name[0] == 'w') ? Kolor_figury.WHITE : Kolor_figury.BLACK;
                switch (figura.picture.Name[1])
                {
                    case 'B':
                        plansza[7 - (int)(figura.position.Y / 100), (int)(figura.position.X / 100)] = new Bishop(k);
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
            
            for (int i = 0; i < historia.Count; i++)
            {
                if (historia[i].i1 == 0 && historia[i].j1 == 0)
                    roszady_bialych[0] = false;
                if (historia[i].i1 == 0 && historia[i].j1 == 7)
                    roszady_bialych[1] = false;
                if (historia[i].i1 == 0 && historia[i].j1 == 4)
                {
                    roszady_bialych[0] = false;
                    roszady_bialych[1] = false;
                }
                if (historia[i].i1 == 7 && historia[i].j1 == 0)
                    roszady_czarnych[0] = false;
                if (historia[i].i1 == 7 && historia[i].j1 == 7)
                    roszady_czarnych[1] = false;
                if (historia[i].i1 == 7 && historia[i].j1 == 4)
                {
                    roszady_bialych[0] = false;
                    roszady_czarnych[1] = false;
                }
            }
            
            //Debug.WriteLine(czy_jest_szach(Kolor_figury.WHITE));
            //Debug.WriteLine(czy_jest_szach(Kolor_figury.BLACK));
            //Debug.WriteLine(czy_atak_na_pole(4, 0, Kolor_figury.BLACK));
            //Debug.WriteLine(czy_atak_na_pole(4, 7, Kolor_figury.WHITE));
        }
        public void Show_Board()
        {
            for (int i = 7; i >= 0; i--)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (plansza[i, j] != null)
                        Debug.Write(plansza[i, j].Show() + " ");
                    else
                        Debug.Write(". ");
                }
                Debug.WriteLine("");
            }
        }
        public void Show_History()
        {
            Debug.WriteLine(historia.Count);

            for (int i = 0; i < historia.Count; i++)
            {
                historia[i].Show();
            }
        }
        public bool czy_atak_na_pole(int i1, int j1, Kolor_figury kolor_atkaujacy) // czy kolor atakuje dane pole
        {
            Figure figura_na_polu = plansza[i1, j1];
            // dodajmy na chwile figure (bo pionki inaczej bija niz sie ruszaja)
            plansza[i1, j1] = new Pawn((kolor_atkaujacy == Kolor_figury.WHITE) ? Kolor_figury.BLACK : Kolor_figury.WHITE);
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (plansza[i, j] != null && plansza[i, j].kolor == kolor_atkaujacy)
                    {
                        List<Move> ruchy = new List<Move>();
                       
                        plansza[i, j].Moves(this, i, j, ruchy, false);


                        bool odp = ruchy.Any(x => x.i2 == i1 && x.j2 == j1);
                        if (odp)
                        {
                            plansza[i1, j1] = figura_na_polu;
                            return true;
                        }
                    }
                }


            }
            plansza[i1, j1] = figura_na_polu;
            return false;
        }
        public Move ostatni_ruch()
        {
            return (historia.Count != 0) ? historia.Last() : null;
        }
        public bool czy_jest_szach(Kolor_figury kolor_atakujacy)
        {
            Kolor_figury kolor_krola = (kolor_atakujacy == Kolor_figury.BLACK) ? Kolor_figury.WHITE : Kolor_figury.BLACK;
            int i_krola=-1;
            int j_krola=-1;
            for(int i=0; i<8; i++)
            {
                for(int j=0; j<8; j++)
                {
                    if (plansza[i, j] == null) continue;
                    if (plansza[i,j].kolor==kolor_krola && plansza[i,j].GetPoints() == 10000)
                    {
                        i_krola = i;
                        j_krola = j;
                        break;
                    }
                }
            }
            if(i_krola==-1)
                return false;
            
            return czy_atak_na_pole(i_krola, j_krola, kolor_atakujacy);
        }
        public bool poprawny_ruch(Move ruch)
        {
            Figure poprzednia = plansza[ruch.i2, ruch.j2];
            plansza[ruch.i2, ruch.j2] = plansza[ruch.i1, ruch.j1];
            plansza[ruch.i1, ruch.j1] = null;
            bool odp = czy_jest_szach((plansza[ruch.i2, ruch.j2].kolor == Kolor_figury.WHITE) ? Kolor_figury.BLACK : Kolor_figury.WHITE);
            plansza[ruch.i1, ruch.j1] = plansza[ruch.i2, ruch.j2];
            plansza[ruch.i2, ruch.j2] = poprzednia;

            return !odp;
        }
    }
    public enum Specjalny {ZWYKLY, ROSZADA_KROTKA_BIALYCH, ROSZADA_DLUGA_BIALYCH,ROSZADA_KROTKA_CZARNYCH, ROSZADA_DLUGA_CZARNYCH, BICIE_W_PRZELOCIE_BIALYCH, BICIE_W_PRZELOCIE_CZARNYCH,PROMOCJA_BIALYCH, PROMOCJA_CZARNYCH}
    internal class Move
    {
        public int i1;
        public int j1;
        public int i2;
        public int j2;
        public Specjalny ruch_specjalny;
        public Move(Vector2 a, Vector2 b)
        {
            i1 = 7 - (int)(a.Y / 100);
            j1 = (int)(a.X / 100);
            i2 = 7 - (int)(b.Y / 100);
            j2 = (int)(b.X / 100);
        }
        public Move(int i1, int j1, int i2, int j2, Specjalny sp=Specjalny.ZWYKLY)
        {
            this.i1 = i1;
            this.j2 = j2;
            this.j1 = j1;
            this.i2 = i2;
            ruch_specjalny = sp;
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
        public static Vector2 IntToVector(int[] tab)
        {
            return new Vector2(tab[1] * 100, (7 - tab[0]) * 100);
        }
        public void Show()
        {
            Debug.WriteLine("("+i1+", "+j1+") ->"+ "(" + i2 + ", " + j2+") "+(ruch_specjalny));
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
        public abstract void Moves(Board b, int i, int j, List<Move> ruchy, bool czy_wlaczyc_szacha);
        public abstract char Show();
    }

    internal class Rook: Figure
    {
        public Rook(Kolor_figury kolor):base(kolor){}
        public override int GetPoints() { return 5; }
        public  override void Moves(Board b, int i, int j, List<Move> ruchy, bool czy_wlaczyc_szacha=true)
        {
            Moves2(b, i, j, ruchy, czy_wlaczyc_szacha) ;
        }
        public static void Moves2(Board b, int i, int j, List<Move> ruchy, bool czy_wlaczyc_szacha=true)
        {
            for (int k = j + 1; k< 8 && dodaj(i, k); k++) ;

            for (int k = j - 1; k >= 0 && dodaj(i, k); k--) ;
            
            for (int k = i + 1; k< 8 && dodaj(k, j); k++) ;
            
            for (int k = i - 1; k >= 0 && dodaj(k, j); k--) ;

            bool dodaj(int i2, int j2)
            {
                if (b.plansza[i2, j2] == null)
                {
                    Move ruch = new Move(i, j, i2, j2);
                    if (b.poprawny_ruch(ruch) || !czy_wlaczyc_szacha)
                        ruchy.Add(ruch);
                    return true;
                }
                else if (b.plansza[i2, j2].kolor != b.plansza[i, j].kolor)
                {
                    Move ruch = new Move(i, j, i2, j2);
                    if (b.poprawny_ruch(ruch) || !czy_wlaczyc_szacha)
                        ruchy.Add(ruch);
                    return false;
                }
                else
                    return false;
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
        public override void Moves(Board b, int i, int j, List<Move> ruchy, bool czy_wlaczyc_szacha = true)
        {
            Rook.Moves2(b, i, j, ruchy, czy_wlaczyc_szacha);
            Bishop.Moves2(b, i, j, ruchy, czy_wlaczyc_szacha);
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
        public override void Moves(Board b, int i, int j, List<Move> ruchy, bool czy_wlaczyc_szacha = true)
        {
            Moves2(b, i, j, ruchy, czy_wlaczyc_szacha);
        }
        public static void Moves2(Board b, int i, int j, List<Move> ruchy, bool czy_wlaczyc_szacha = true)
        {
            dodaj(-1,-1);
            dodaj(-1,1);
            dodaj(1,1);
            dodaj(1,-1);
            void dodaj(int plus_i, int plus_j)
            {
                Move ruch;
                int k = 1;
                while (true)
                {
                    if (k * plus_i + i > 7 || k * plus_i + i < 0)
                        break;
                    if (k * plus_j + j > 7 || k * plus_j + j < 0)
                        break;
                    if (b.plansza[k * plus_i + i, k * plus_j + j]!=null && b.plansza[k * plus_i + i, k * plus_j + j].kolor == b.plansza[i, j].kolor)
                        break;
                    if (b.plansza[k * plus_i + i, k * plus_j + j] != null && b.plansza[k * plus_i + i, k * plus_j + j].kolor != b.plansza[i, j].kolor)
                    {
                        ruch = new Move(i, j, k * plus_i + i, k * plus_j + j);
                        if (b.poprawny_ruch(ruch) || !czy_wlaczyc_szacha)
                            ruchy.Add(ruch);
                        break;
                    }
                    ruch = new Move(i, j, k * plus_i + i, k * plus_j + j);
                    if (b.poprawny_ruch(ruch) || !czy_wlaczyc_szacha)
                        ruchy.Add(ruch);
                    k++;
                }
            }
                
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
        public override void Moves(Board b, int i, int j, List<Move> ruchy, bool czy_wlaczyc_szacha = true)
        {
            Moves2(b, i, j, ruchy, czy_wlaczyc_szacha);
        }
        public static void Moves2(Board b, int i, int j, List<Move> ruchy, bool czy_wlaczyc_szacha = true)
        {
            dodaj(-2, -1);
            dodaj(-2, 1);
            dodaj(2, 1);
            dodaj(2, -1);
            dodaj(1, -2);
            dodaj(1, 2);
            dodaj(-1, -2);
            dodaj(-1, 2);
            void dodaj(int plus_i, int plus_j)
            {
                Move ruch;
                if(plus_i+i<8 && plus_i+i>=0 &&plus_j + j < 8 && plus_j + j >= 0)
                {
                    if (b.plansza[i + plus_i, j + plus_j] ==null)
                    {
                        ruch = new Move(i, j, plus_i + i, plus_j + j);
                        if (b.poprawny_ruch(ruch) || !czy_wlaczyc_szacha)
                            ruchy.Add(ruch);
                    }
                    if (b.plansza[i + plus_i, j + plus_j] != null && b.plansza[i,j].kolor!= b.plansza[i+plus_i, j+plus_j].kolor)
                    {
                        ruch = new Move(i, j, plus_i + i, plus_j + j);
                        if (b.poprawny_ruch(ruch) || !czy_wlaczyc_szacha)
                            ruchy.Add(ruch);
                    }
                        
                }
                 
            }
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
        public override void Moves(Board b, int i, int j, List<Move> ruchy, bool czy_wlaczyc_szacha = true)
        {
            Moves2(b, i, j, ruchy, czy_wlaczyc_szacha);
        }
        public static void Moves2(Board b, int i, int j, List<Move> ruchy, bool czy_wlaczyc_szacha = true)
        {
            dodaj(1, -1);
            dodaj(1, 1);
            dodaj(1, 0);
            dodaj(-1, -1);
            dodaj(-1, 1);
            dodaj(-1, 0);
            dodaj(0, 1);
            dodaj(0,-1);
            void dodaj(int plus_i, int plus_j)
            {
                Move ruch;
                if (plus_i + i < 8 && plus_i + i >= 0 && plus_j + j < 8 && plus_j + j >= 0)
                {
                    if (b.plansza[i + plus_i, j + plus_j] == null)
                    {
                        ruch = new Move(i, j, plus_i + i, plus_j + j);
                        if (b.poprawny_ruch(ruch) || !czy_wlaczyc_szacha)
                            ruchy.Add(ruch);
                    } 
                    if (b.plansza[i + plus_i, j + plus_j] != null && b.plansza[i, j].kolor != b.plansza[i + plus_i, j + plus_j].kolor)
                    {
                        ruch = new Move(i, j, plus_i + i, plus_j + j);
                        if (b.poprawny_ruch(ruch) || !czy_wlaczyc_szacha)
                            ruchy.Add(ruch);
                    }  
                }

            }
            // Roszady
            if (b.plansza[i,j].kolor==Kolor_figury.WHITE && !b.czy_jest_szach(Kolor_figury.BLACK))
            {
                if (b.roszady_bialych[0] && b.plansza[0,3]==null && b.plansza[0, 2] == null  && b.plansza[0, 1] == null && !b.czy_atak_na_pole(0,3,Kolor_figury.BLACK) && !b.czy_atak_na_pole(0, 2, Kolor_figury.BLACK))
                {
                    ruchy.Add(new Move(i, j, 0,2, Specjalny.ROSZADA_DLUGA_BIALYCH));
                }
                if (b.roszady_bialych[1] && b.plansza[0, 5] == null && b.plansza[0, 6] == null  && !b.czy_atak_na_pole(0, 5, Kolor_figury.BLACK) && !b.czy_atak_na_pole(0, 6, Kolor_figury.BLACK))
                {
                    ruchy.Add(new Move(i, j, 0, 6, Specjalny.ROSZADA_KROTKA_BIALYCH));
                }
            }
            if (b.plansza[i, j].kolor == Kolor_figury.BLACK && !b.czy_jest_szach(Kolor_figury.WHITE))
            {
                if (b.roszady_czarnych[0] && b.plansza[7, 3] == null && b.plansza[7, 2] == null && b.plansza[7, 1] == null && !b.czy_atak_na_pole(7, 3, Kolor_figury.WHITE) && !b.czy_atak_na_pole(7, 2, Kolor_figury.WHITE))
                {
                    ruchy.Add(new Move(i, j, 7, 2, Specjalny.ROSZADA_DLUGA_CZARNYCH));
                }
                if (b.roszady_czarnych[1] && b.plansza[7, 5] == null && b.plansza[7, 6] == null && !b.czy_atak_na_pole(7, 5, Kolor_figury.WHITE) && !b.czy_atak_na_pole(7, 6, Kolor_figury.WHITE))
                {
                    ruchy.Add(new Move(i, j, 7, 6, Specjalny.ROSZADA_KROTKA_CZARNYCH));
                }
            }
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
        public override void Moves(Board b, int i, int j, List<Move> ruchy, bool czy_wlaczyc_szacha = true)
        {
            Moves2(b, i, j, ruchy, czy_wlaczyc_szacha);
        }
        public static void Moves2(Board b, int i, int j, List<Move> ruchy, bool czy_wlaczyc_szacha = true)
        {
            Move ruch;
            if (b.plansza[i, j].kolor == Kolor_figury.WHITE) // biale
            {
                if (i == 1 && b.plansza[i + 2, j] == null && b.plansza[i + 1, j] == null) // ruch o dwa
                {
                    ruch = new Move(i, j, 2 + i, j);
                    if (b.poprawny_ruch(ruch) || !czy_wlaczyc_szacha)
                        ruchy.Add(ruch);
                }
                if (i + 1 < 8 && b.plansza[i + 1, j] == null) // ruch o jeden
                {
                    ruch = new Move(i, j, 1 + i, j, (1 + i == 7) ? Specjalny.PROMOCJA_BIALYCH : Specjalny.ZWYKLY);
                    if (b.poprawny_ruch(ruch) || !czy_wlaczyc_szacha)
                        ruchy.Add(ruch);
                }
                if (i + 1 < 8 && j - 1 >= 0 && b.plansza[i + 1, j - 1] != null && b.plansza[i + 1, j - 1].kolor == Kolor_figury.BLACK) // ruch o bicie w lewo
                {
                    ruch = new Move(i, j, 1 + i, j - 1, (1 + i == 7) ? Specjalny.PROMOCJA_BIALYCH : Specjalny.ZWYKLY);
                    if (b.poprawny_ruch(ruch) || !czy_wlaczyc_szacha)
                        ruchy.Add(ruch);
                }
                if (i + 1 < 8 && j + 1 < 8 && b.plansza[i + 1, j + 1] != null && b.plansza[i + 1, j + 1].kolor == Kolor_figury.BLACK) // ruch o bicie w prawo
                {
                    ruch = new Move(i, j, 1 + i, j + 1, (1 + i == 7) ? Specjalny.PROMOCJA_BIALYCH : Specjalny.ZWYKLY);
                    if (b.poprawny_ruch(ruch) || !czy_wlaczyc_szacha)
                        ruchy.Add(ruch);
                }
                //bicie w przelocie
                Move ostatni_ruch = b.ostatni_ruch();
                if (ostatni_ruch != null && ostatni_ruch.i2 == 4 && ostatni_ruch.i1 == 6 && b.plansza[ostatni_ruch.i2, ostatni_ruch.j2] != null && b.plansza[ostatni_ruch.i2, ostatni_ruch.j2].kolor == Kolor_figury.BLACK && b.plansza[ostatni_ruch.i2, ostatni_ruch.j2].GetType() == b.plansza[i, j].GetType())
                {

                    if (j == ostatni_ruch.j2 - 1 && i == ostatni_ruch.i2)
                    {
                        ruch = new Move(i, j, i + 1, j + 1, Specjalny.BICIE_W_PRZELOCIE_BIALYCH);
                        if (b.poprawny_ruch(ruch))
                            ruchy.Add(ruch);
                    }
                    if (j == ostatni_ruch.j2 + 1 && i == ostatni_ruch.i2)
                    {
                        ruch = new Move(i, j, i + 1, j - 1, Specjalny.BICIE_W_PRZELOCIE_BIALYCH);
                        if (b.poprawny_ruch(ruch))
                            ruchy.Add(ruch);
                    }

                }
            }
            else // czarne
            {
                if (i == 6 && b.plansza[i - 2, j] == null && b.plansza[i - 1, j] == null) // ruch o dwa
                {
                    ruch = new Move(i, j, i - 2, j);
                    if (b.poprawny_ruch(ruch) || !czy_wlaczyc_szacha)
                        ruchy.Add(ruch);
                }
                if (i - 1 >= 0 && b.plansza[i - 1, j] == null) // ruch o jeden
                {
                    ruch = new Move(i, j, i - 1, j, (i - 1 == 0) ? Specjalny.PROMOCJA_CZARNYCH : Specjalny.ZWYKLY);
                    if (b.poprawny_ruch(ruch) || !czy_wlaczyc_szacha)
                        ruchy.Add(ruch);
                }
                if (i - 1 >= 0 && j - 1 >= 0 && b.plansza[i - 1, j - 1] != null && b.plansza[i - 1, j - 1].kolor == Kolor_figury.WHITE) // ruch o bicie w lewo
                {
                    ruch = new Move(i, j, i - 1, j - 1, (i - 1 == 0) ? Specjalny.PROMOCJA_CZARNYCH : Specjalny.ZWYKLY);
                    if (b.poprawny_ruch(ruch) || !czy_wlaczyc_szacha)
                        ruchy.Add(ruch);
                }
                if (i - 1 >= 0 && j + 1 < 8 && b.plansza[i - 1, j + 1] != null && b.plansza[i - 1, j + 1].kolor == Kolor_figury.WHITE) // ruch o bicie w prawo
                {
                    ruch = new Move(i, j, i - 1, j + 1, (i - 1 == 0) ? Specjalny.PROMOCJA_CZARNYCH : Specjalny.ZWYKLY);
                    if (b.poprawny_ruch(ruch) || !czy_wlaczyc_szacha)
                        ruchy.Add(ruch);
                }
                
                Move ostatni_ruch = b.ostatni_ruch();
                if (ostatni_ruch != null && ostatni_ruch.i2 == 3 && ostatni_ruch.i1 == 1 && b.plansza[ostatni_ruch.i2, ostatni_ruch.j2]!=null && b.plansza[ostatni_ruch.i2, ostatni_ruch.j2].kolor == Kolor_figury.WHITE && b.plansza[ostatni_ruch.i2, ostatni_ruch.j2].GetType() == b.plansza[i, j].GetType())
                {

                    if (j == ostatni_ruch.j2 - 1 && i == ostatni_ruch.i2)
                    {
                        ruch = new Move(i, j, i - 1, j + 1, Specjalny.BICIE_W_PRZELOCIE_CZARNYCH);
                        if (b.poprawny_ruch(ruch))
                            ruchy.Add(ruch);
                    }

                    if (j == ostatni_ruch.j2 + 1 && i == ostatni_ruch.i2)
                    {
                        ruch = new Move(i, j, i - 1, j - 1, Specjalny.BICIE_W_PRZELOCIE_CZARNYCH);
                        if (b.poprawny_ruch(ruch))
                            ruchy.Add(ruch);
                    }

                }
            }
        }
        public override char Show()
        {
            return (kolor == Kolor_figury.WHITE) ? 'P' : 'p';
        }
    }
}
