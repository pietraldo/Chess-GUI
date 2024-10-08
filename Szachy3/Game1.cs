﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;

namespace Szachy3
{
    public enum Kolor_figury { WHITE, BLACK };
    public class Game1 : Game
    {
        
        public class Piece
        {
            public static readonly int orginal_piece_size = 100;
            public Texture2D picture;
            public Vector2 position;
            public float curent_size=1;
            public Kolor_figury kolor;
            public readonly int id;
            private static int liczba_figur=0;

            // do animacji
            public bool zbity = false;
            public float elapsedTime=0;
            public Vector2 przemieszczenie;
            public Vector2 miejsce_docelowe;

            // do animacji promocji
            public bool promocja = false;
            public Vector2 position_promocji;
            public float obrot = 0;

            public Piece(Texture2D tekstura)
            { 
                picture = tekstura;
                id = liczba_figur++;
            }
            public Piece(Texture2D tekstura, Vector2 position)
            {
                picture = tekstura;
                this.position= position;
                id = liczba_figur++;
            }

            public bool IsMouseOver(MouseState mouseState)
            {
                Rectangle objectBounds = new Rectangle((int)position.X + board_offset_x, (int)position.Y + board_offset_y, picture.Width, picture.Height);
                return objectBounds.Contains(mouseState.X, mouseState.Y);
            }
          
            public void AnimacjaZbicia(float game_elapsed_time)
            {
                // Animacja zmniejszania
                elapsedTime += game_elapsed_time;

                if (elapsedTime <= animationDuration)
                {
                    float t = elapsedTime / animationDuration;
                    curent_size = MathHelper.Lerp(1f, targetScale, t);
                }
                else
                    curent_size = targetScale;

                // Animacja przejscia
                if (position == miejsce_docelowe) return;

                float distance = Vector2.Distance(position, miejsce_docelowe);
                float moveAmount = moveSpeed * game_elapsed_time;
                if (distance > moveAmount)
                    position += przemieszczenie * moveAmount;
                else
                    position = miejsce_docelowe; // Docelowa pozycja osiągnięta
            }



            private bool etap1 = false;
            static float targetScale2 = 4f;          // Target scale 
            private float animationDuration2=2.4f;    // Animation duration in seconds
            private float elapsedTime2 = 0;
            private float elapsedTime3 = 0;
            public void AnimacjaPromocji(float time)
            {
                float moveSpeed2 = 500;
                //Etap 1 powiekszenie i krecenie
                if(!etap1)
                {
                    elapsedTime2 += time;

                    if(elapsedTime2<=animationDuration2)
                    {
                        // krecenie sie
                        obrot += MathHelper.ToRadians(4000) * time;
                        // poruszanie
                        przemieszczenie = new Vector2(400, 400) - position;
                        przemieszczenie.Normalize();
                        float moveAmount = moveSpeed2 * time;
                        if (Vector2.Distance(new Vector2(400, 400), position) > 10)
                                position += przemieszczenie * moveAmount;
                        // powiekszanie
                        float t = elapsedTime2 / animationDuration2;
                        curent_size = MathHelper.Lerp(1f, targetScale2, t);
                    }
                    else
                    {
                        etap1 = true;
                        obrot = 0;
                        
                        position = new Vector2(400-picture.Width*curent_size/2, 400-picture.Width*curent_size/2);
                        Debug.WriteLine(position.X + " " + position.Y);
                        moveSpeed2 = Vector2.Distance(position, position_promocji) / animationDuration2;
                        przemieszczenie = position_promocji - position;
                        przemieszczenie.Normalize();
                    }
                }
                else
                {
                    picture =(kolor==Kolor_figury.WHITE)?do_promocji_w:do_promocji_b;
                    elapsedTime3 += time;
                    if(elapsedTime3<=animationDuration2)
                    {
                        // poruszanie
                        float moveAmount = moveSpeed2 * time;
                        if (Vector2.Distance(position_promocji, position) > moveAmount)
                            position += przemieszczenie * moveAmount;
                        else
                            position = position_promocji;

                        // powiekszanie
                        float t = elapsedTime3 / animationDuration2;
                        curent_size = MathHelper.Lerp(targetScale2, 1f, t);
                    }
                    else
                    {
                        curent_size = 1;
                        position = position_promocji;
                        obrot = 0;
                        promocja = false;
                    }
                }

            }
        }

        // ustawienia
        private string fileWithSettings = "Settings.json";
        public bool block_mouse = false;

        private string pozycja_startowa= "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1";
        public bool wylaczyc_kolejnosc = false;
        public Kolor_figury czyja_kolej = Kolor_figury.WHITE;
        public bool mute = false;
        public bool bot = false;
        public static string engine_path = "stockfish.exe";
        public static string engine_depth = "4";
        public static Kolor_figury engine_color=Kolor_figury.BLACK;

        // board
        Texture2D board;
        Vector2 board_position;
        int board_start_position_x = 0;
        int board_start_position_y = 0;
        public int zbite_biale=0;
        public int zbite_czarne=0;
        public bool obroc_plansze = false;
        private const int window_width = 1300;
        private const int window_height = 900;
        public static int board_offset_x = 50;
        public static int board_offset_y = 50;
        public Vector2[] pola_bota= new Vector2[0]; 

        private List<Move> mozliwe_posuniecia= new List<Move>();
        private List<Move> historia = new List<Move>();

        // tablica figur
        public Piece[] figury;
        public int liczba_figur;

        // mouse dragg piece
        public int index_dragged_piece = -1;
        public Vector2 prev_position;
        
        // game
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // sound
        Song move_sound;
        Song wrong_move_sound;
        Song capture_sound;
        Song promotion_sound;
        Song laugh;

        // button
        Texture2D button1;
        Vector2 button1_position;
        int button1_start_position_x = 1000;
        int button1_start_position_y = 350;

        // animation     
        static float targetScale = 0.3f;          // Target scale 
        static float animationDuration = 0.2f;    // Animation duration in seconds
        static float moveSpeed = 5000.0f;
        public static Texture2D do_promocji_w;
        public static Texture2D do_promocji_b;

        // animacja matowania
        bool koniec_gry = false;
        Piece krol_matowany;
        bool mat = true;
        int numer_obrazka = 1;
        int animation_end_speed = 10;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _graphics.PreferredBackBufferWidth = window_width; 
            _graphics.PreferredBackBufferHeight = window_height;
        }

        protected override void Initialize()
        {
            LoadUserSettings();

            Initialize_Sounds();
            Initialize_Board();
            Initialize_Pieces(pozycja_startowa);
            Initialize_Buttons();

            zdobadz_ruchy();

            do_promocji_w = Content.Load<Texture2D>("wQ");
            do_promocji_b = Content.Load<Texture2D>("bQ");

            base.Initialize();
        }

        private void LoadUserSettings()
        {
            string jsonString = File.ReadAllText(fileWithSettings);
            dynamic data = JObject.Parse(jsonString);

            try
            {
                if(!File.Exists(fileWithSettings))
                    throw new Exception("Setting file not found");

                pozycja_startowa = data.start_position ?? pozycja_startowa;
                wylaczyc_kolejnosc = data.disable_order ?? wylaczyc_kolejnosc;
                string kolej= data.whose_turn??"WHITE";
                if(kolej== "WHITE")
                    czyja_kolej = Kolor_figury.WHITE;
                else if(kolej=="BLACK")
                    czyja_kolej = Kolor_figury.BLACK;
                mute = data.mute ?? mute;
                bot = data.play_with_engine ?? bot;
                engine_path = data.engine_path ?? engine_path;
                engine_depth = data.engine_depth?? engine_depth;
                string engine_colorSetting = data.engine_color ?? "BLACK";
                if (engine_colorSetting == "WHITE")
                    engine_color = Kolor_figury.WHITE;
                else if (engine_colorSetting == "BLACK")
                    engine_color = Kolor_figury.BLACK;

                if(bot && !File.Exists(engine_path))
                {
                    throw new Exception("Turn off playing with engine or specify correct engine path");
                }

            }
            catch (Exception e)
            {
                ShowMessageBox(e.Message, "Error in settings");
                Exit();
            }
        }

        

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }
     
        protected override void Update(GameTime gameTime)
        {
            // ustawianie dzwiekow
            MediaPlayer.IsMuted=mute;

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) //wychodznie klawiszami z aplikacji
                Exit();

            if (Keyboard.GetState().IsKeyDown(Keys.Q)) // do promocji
            {
                do_promocji_w = Content.Load<Texture2D>("wQ");
                do_promocji_b = Content.Load<Texture2D>("bQ");
            }
            if (Keyboard.GetState().IsKeyDown(Keys.R)) // do promocji
            {
                do_promocji_w = Content.Load<Texture2D>("wR");
                do_promocji_b = Content.Load<Texture2D>("bR");
            }
            if (Keyboard.GetState().IsKeyDown(Keys.N)) // do promocji
            {
                do_promocji_w = Content.Load<Texture2D>("wN");
                do_promocji_b = Content.Load<Texture2D>("bN");
            }
            

            MouseState mouseState = Mouse.GetState();

            // wyswietlanie kursora raczki nad figura
            Mouse.SetCursor(figury.Any(figura => figura.IsMouseOver(mouseState)) ? MouseCursor.Hand : MouseCursor.Arrow);

            // lapanie figur
            lapanie_myszka_figur(mouseState);

            // klikanie przycisku - resetuj
            if (mouseState.LeftButton == ButtonState.Pressed && (new Rectangle((int)button1_position.X, (int)button1_position.Y, button1.Width, button1.Height)).Contains(mouseState.X, mouseState.Y))
                klikniecie_przycisku_restart();
            
            // Wlaczenie animacji zbicia - dla elementow ktore sa zbite
            figury.Where(x => x.zbity).ToList().ForEach(x => x.AnimacjaZbicia((float)gameTime.ElapsedGameTime.TotalSeconds));

            // Wlaczenie animacji promocji 
            figury.Where(x => x.promocja).ToList().ForEach(x => x.AnimacjaPromocji((float)gameTime.ElapsedGameTime.TotalSeconds));

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            // Rysowanie elementów
            _spriteBatch.Begin();

            // Rysowanie planszy
            _spriteBatch.Draw(board, new Vector2(board_position.X+board_offset_x, board_position.Y+board_offset_y), Color.White);
           
            // Rysowanie czerwonych pól
            rysuj_mozliwe_ruchy(mozliwe_posuniecia);
            rysuj_czerowne_pola(pola_bota);
            
            // Rysowanie przycisku
            _spriteBatch.Draw(button1, button1_position, Color.White);

            // Rysowanie figur
            for (int i = 0; i < liczba_figur; i++)
            {
                if (figury[i].promocja) continue;
                    rysuj_figure(figury[i]);
            }
                
            
            // Rysowanie pionka trzymanego przez myszke na samej gorze
            if (index_dragged_piece != -1)
                rysuj_figure(figury[index_dragged_piece], 128);

            var promowany = figury.Where(x => x.promocja);
            foreach (Piece a in promowany)
                rysuj_figure(a,255);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
        

        public void zdobadz_ruchy()
        {
            Board b = new Board(figury,historia);
            //b.Show_Board();
            //b.Show_History();

            
            Vector2 m = new Vector2(Mouse.GetState().X-board_offset_x, Mouse.GetState().Y-board_offset_y);
            
            
            List<Move> avible_moves = new List<Move>();

            int[] wsp = Move.VectorToInt(m);

            if (wsp[0]<8 && wsp[0]>=0 && wsp[1]<8 && wsp[1]>=0 && b.plansza[wsp[0], wsp[1]] != null)
            {
                b.plansza[wsp[0], wsp[1]].Moves(b, wsp[0], wsp[1], avible_moves,true);
            }

            mozliwe_posuniecia = avible_moves;

        }
        public void lapanie_myszka_figur(MouseState mouseState)
        {
            if (koniec_gry)
            {
                animacja_konca_gry();
                return;
            }
            if (figury.Any(x => x.promocja) || figury.Any(x=> (x.zbity && (x.position!=x.miejsce_docelowe)) )) return;
            if(bot && czyja_kolej==engine_color)
            {
                bot_zrob_ruch();
            }
            if (block_mouse) return;
            if (mouseState.LeftButton == ButtonState.Pressed) //klikniecie mysza
            {
                if (index_dragged_piece == -1) //jak jeszcze niczego nie trzyma
                {
                    // Wybieranie figury nad ktora jest myszka i ustawianie indexu i prev_position
                    var el = figury.Select((figura, index) => new {  Index = index, Figura = figura }).FirstOrDefault( x => x.Figura.IsMouseOver(mouseState));
                    if(el!=null && (figury[el.Index].kolor==czyja_kolej || wylaczyc_kolejnosc))
                    {
                        
                        if (figury[el.Index].zbity == false)
                            zdobadz_ruchy();

                        index_dragged_piece = el.Index;
                        prev_position = el.Figura.position;
                    }
                   
                }
                else // jak trzyma figure -> aktualizuj pozycje pionka
                    figury[index_dragged_piece].position = new Vector2(mouseState.X - board_offset_x - Piece.orginal_piece_size / 2, mouseState.Y - board_offset_y - Piece.orginal_piece_size / 2);
               
            }
            else // przycisk myszy nie klikniety
            {
                if (index_dragged_piece != -1) // Puszczenie pionka
                {
                    
                    // dopasowanie pozycji pionka do pola po puszczeniu
                    figury[index_dragged_piece].position = new Vector2((int)((figury[index_dragged_piece].position.X + Piece.orginal_piece_size / 2) / 100) * 100, (int)((figury[index_dragged_piece].position.Y + Piece.orginal_piece_size / 2) / 100) * 100);

                    if(mozliwe_posuniecia!=null&& mozliwe_posuniecia.Any(x => x==znajdz_wykonany_ruch())) // czy ta figura moze sie na to pole ruszyc
                    {
                        // szukanie czy na polu postawionego pionka jest jakis inny
                        Piece zbita = figury.FirstOrDefault(x => x.position == figury[index_dragged_piece].position && x.id != figury[index_dragged_piece].id);

                        if (zbita == null) // puste pole
                        {
                            MediaPlayer.Play(move_sound);
                            
                            // czy moze to roszada
                            sprawdz_czy_byla_roszada();
                           
                        }
                        else //pole z figura
                        {
                             // pole z figura przeciwnika
                            zbicie_przeciwnika(zbita);
                        }

                        // bicie w przelocie
                        sprawdz_czy_bylo_bicie_w_przelocie();

                        historia.Add(znajdz_wykonany_ruch());

                        // zmiana gracza
                        czyja_kolej = (czyja_kolej == Kolor_figury.WHITE) ? Kolor_figury.BLACK : Kolor_figury.WHITE;
                        
                        koniec_gry=czy_koniec_gry();
                        //wyswietl_historie();
                    }
                    else // zle upuszczenie figury albo w tym samym miejscu
                    {
                        if (figury[index_dragged_piece].position!=prev_position) 
                            MediaPlayer.Play(wrong_move_sound);

                        figury[index_dragged_piece].position = prev_position;
                    }


                    //promocja
                    promocja(figury[index_dragged_piece]);

                    mozliwe_posuniecia = new List<Move>(); //resetowanie ruchow 
                }
                
                index_dragged_piece = -1;
            }
        }

        public void promocja(Piece pionek)
        {
            if (pionek.picture.Name[1] == 'P' && pionek.kolor == Kolor_figury.WHITE && pionek.position.Y == 0)
            {
                pionek.position_promocji = pionek.position;
                pionek.promocja = true;
                MediaPlayer.Play(promotion_sound);
            }
            else if (pionek.picture.Name[1] == 'P' && pionek.kolor == Kolor_figury.BLACK && pionek.position.Y == 700)
            {
                pionek.position_promocji = pionek.position;
                pionek.promocja = true;
                MediaPlayer.Play(promotion_sound);
            }
        }
        private void bot_zrob_ruch()
        {
            Board b = new Board(figury, historia);
            Bot bot = new Bot();
            Move ruchBota = bot.MakeMoveFromFileEngine(b);
            if (ruchBota != null)
            {
                var el = figury.Select((figura, index) => new { Index = index, Figura = figura }).FirstOrDefault(x => x.Figura.position == Move.IntToVector(new int[] { ruchBota.i1, ruchBota.j1 }));
                Piece zbita = figury.FirstOrDefault(x => x.position == Move.IntToVector(new int[] { ruchBota.i2, ruchBota.j2 }) );

                if (zbita != null)  //pole z figura
                {
                    zbicie_przeciwnika(zbita);
                }
                else
                {
                    MediaPlayer.Play(move_sound);
                }
                
                pola_bota=(new Vector2[] { Move.IntToVector(new int[] { ruchBota.i2, ruchBota.j2 }) , Move.IntToVector(new int[] { ruchBota.i1, ruchBota.j1 }) });
                el.Figura.position = Move.IntToVector(new int[] { ruchBota.i2, ruchBota.j2 });
                sprawdz_czy_byla_roszada(ruchBota);
                sprawdz_czy_bylo_bicie_w_przelocie(ruchBota);
                
                promocja(el.Figura);
                historia.Add(ruchBota);
                //wyswietl_historie();
                
            }
            czyja_kolej = engine_color== Kolor_figury.WHITE?Kolor_figury.BLACK:Kolor_figury.WHITE;
            koniec_gry = czy_koniec_gry();
        }
        private Move znajdz_wykonany_ruch()
        {
            return mozliwe_posuniecia.FirstOrDefault(x => { return figury[index_dragged_piece].position == Move.IntToVector(new int[] { x.i2, x.j2 }); });
        }
        private void wyswietl_historie()
        {
            Debug.WriteLine("---------Historia---------");
            for(int i=0; i<historia.Count; i++)
            {
                Debug.WriteLine($"{i+1}.({historia[i].i1}, {historia[i].j1}) ->({historia[i].i2}, {historia[i].j2}) - {historia[i].ruch_specjalny}");
            }
            Debug.WriteLine("---------------------------");
        }
        private void sprawdz_czy_byla_roszada(Move ruch2=null)
        {
            Move ruch;
            if (ruch2 == null)
            {
                ruch = znajdz_wykonany_ruch();
            }
            else ruch = ruch2;
             
            if (ruch == null) { return; }
            if (ruch.ruch_specjalny==Specjalny.ROSZADA_DLUGA_BIALYCH)
            {
                Piece wieza = figury.FirstOrDefault(x => x.position == new Vector2(0, 700));
                wieza.position = new Vector2(300, 700);
            }
            else if (ruch.ruch_specjalny == Specjalny.ROSZADA_KROTKA_BIALYCH)
            {
                Piece wieza = figury.FirstOrDefault(x => x.position == new Vector2(700, 700));
                wieza.position = new Vector2(500, 700);
            }

            if (ruch.ruch_specjalny == Specjalny.ROSZADA_DLUGA_CZARNYCH)
            {
                Piece wieza = figury.FirstOrDefault(x => x.position == new Vector2(0, 0));
                wieza.position = new Vector2(300, 0);
            }
            else if (ruch.ruch_specjalny == Specjalny.ROSZADA_KROTKA_CZARNYCH)
            {
                Piece wieza = figury.FirstOrDefault(x => x.position == new Vector2(700, 0));
                wieza.position = new Vector2(500, 0);
            }
        }
        private void sprawdz_czy_bylo_bicie_w_przelocie(Move ruch2=null)
        {
            Move ruch;
            if (ruch2 == null)
                ruch = znajdz_wykonany_ruch();
            else ruch = ruch2;
           
            if (ruch != null && ruch.ruch_specjalny == Specjalny.BICIE_W_PRZELOCIE_BIALYCH)
            {
                Piece zbita_w_przelocie = figury.FirstOrDefault(x => x.position == Move.IntToVector(new int[] { ruch.i2 - 1, ruch.j2 }));
                zbicie_przeciwnika(zbita_w_przelocie);
            }
            if (ruch != null && ruch.ruch_specjalny == Specjalny.BICIE_W_PRZELOCIE_CZARNYCH)
            {
                Piece zbita_w_przelocie = figury.FirstOrDefault(x => x.position == Move.IntToVector(new int[] { ruch.i2 + 1, ruch.j2 }));
                zbicie_przeciwnika(zbita_w_przelocie);
            }
        }
       
        public void zbicie_przeciwnika(Piece zbity)
        {
            MediaPlayer.Play(capture_sound);
            if (zbity.kolor == Kolor_figury.WHITE)
            {
                zbity.miejsce_docelowe = new Vector2(board.Width+board_offset_x + (zbite_biale%6)*30, board.Height - 30*(1+ ((int)(zbite_biale/6)) )   );
                zbite_biale++;
            }
            else
            {
                zbity.miejsce_docelowe = new Vector2(board.Width + board_offset_x + (zbite_czarne % 6) * 30,   30 *  ((int)(zbite_czarne / 6)));
                zbite_czarne++;
            }
            zbity.zbity = true;
            zbity.przemieszczenie = zbity.miejsce_docelowe - zbity.position;
            zbity.przemieszczenie.Normalize();
        }
        protected void Initialize_Board()
        {
            board = Content.Load<Texture2D>("board");
            board_position = new Vector2(board_start_position_x, board_start_position_y);
        }
        protected void Initialize_Pieces(string FEN= "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR b KQkq - 0 1")
        {
            zbite_biale = 0;
            zbite_czarne = 0;

            liczba_figur = 64;
            FEN = FEN.Split(' ')[0];
            FEN.Where(x => x - '0' >= 0 && x - '0' <= 9).ToList().ForEach(x => { liczba_figur -= (x - '0');  });

            figury = new Piece[liczba_figur];
            int pozycja_na_planszy = 0;
            int nr_figury = 0;
            for(int i=0; i<FEN.Length; i++)
            {
                switch(FEN[i])
                {
                    case 'r':
                        figury[nr_figury++] = new Piece(Content.Load<Texture2D>("bR"));
                        zainicjuj(Kolor_figury.BLACK);
                        break;
                    case 'b':
                        figury[nr_figury++] = new Piece(Content.Load<Texture2D>("bB"));
                        zainicjuj(Kolor_figury.BLACK);
                        break;
                    case 'k':
                        figury[nr_figury++] = new Piece(Content.Load<Texture2D>("bK"));
                        zainicjuj(Kolor_figury.BLACK);
                        break;
                    case 'q':
                        figury[nr_figury++] = new Piece(Content.Load<Texture2D>("bQ"));
                        zainicjuj(Kolor_figury.BLACK);
                        break;
                    case 'p':
                        figury[nr_figury++] = new Piece(Content.Load<Texture2D>("bP"));
                        zainicjuj(Kolor_figury.BLACK);
                        break;
                    case 'n':
                        figury[nr_figury++] = new Piece(Content.Load<Texture2D>("bN"));
                        zainicjuj(Kolor_figury.BLACK);
                        break;
                    case 'N':
                        figury[nr_figury++] = new Piece(Content.Load<Texture2D>("wN"));
                        zainicjuj(Kolor_figury.WHITE);
                        break;
                    case 'B':
                        figury[nr_figury++] = new Piece(Content.Load<Texture2D>("wB"));
                        zainicjuj(Kolor_figury.WHITE);
                        break;
                    case 'K':
                        figury[nr_figury++] = new Piece(Content.Load<Texture2D>("wK"));
                        zainicjuj(Kolor_figury.WHITE);
                        break;
                    case 'Q':
                        figury[nr_figury++] = new Piece(Content.Load<Texture2D>("wQ"));
                        zainicjuj(Kolor_figury.WHITE);
                        break;
                    case 'R':
                        figury[nr_figury++] = new Piece(Content.Load<Texture2D>("wR"));
                        zainicjuj(Kolor_figury.WHITE);
                        break;
                    case 'P':
                        figury[nr_figury++] = new Piece(Content.Load<Texture2D>("wP"));
                        zainicjuj(Kolor_figury.WHITE);
                        break;
                    case '/':
                        break;
                    default:
                        pozycja_na_planszy += (FEN[i] - '0');
                        break;

                }
            }

            void zainicjuj(Kolor_figury kol)
            {
                figury[nr_figury-1].kolor = kol;
                figury[nr_figury-1].position = new Vector2(pozycja_na_planszy % 8 * 100, (int)(pozycja_na_planszy / 8) * 100);
                pozycja_na_planszy++;
            }
        }
        protected void Initialize_Sounds()
        {
            this.move_sound = Content.Load<Song>("tech_sound");
            this.wrong_move_sound = Content.Load<Song>("sound2");
            this.capture_sound = Content.Load<Song>("explo_sound");
            this.promotion_sound = Content.Load<Song>("win3");
            this.laugh = Content.Load<Song>("laugh");
        }
        protected void Initialize_Buttons()
        {
            button1 = Content.Load<Texture2D>("resetuj");
            button1_position = new Vector2(button1_start_position_x, button1_start_position_y);
        }

        private void rysuj_mozliwe_ruchy(List<Move> ruchy)
        {
            for (int i = 0; i < ruchy.Count; i++)
            {
                Texture2D czerwony = Content.Load<Texture2D>("niebieski");

                _spriteBatch.Draw(czerwony, Move.IntToVector(new int[2] { ruchy[i].i2, ruchy[i].j2 })+new Vector2( board_offset_x, board_offset_y), new Color(255, 255, 255, 128)); // 50% przezroczystości

            }
        }

        protected void rysuj_czerowne_pola(Vector2[] pola)
        {
            for(int i=0; i< pola.Length; i++) 
            {
                Texture2D czerwony = Content.Load<Texture2D>("czerwone");

                _spriteBatch.Draw(czerwony, new Vector2(pola[i].X + board_offset_x, pola[i].Y+board_offset_y), new Color(255,255,255,128)); // 50% przezroczystości

            }
        }
        protected void rysuj_figure(Piece p, int transparency=255)
        {
            if (p.obrot == 0)
                _spriteBatch.Draw(p.picture, new Vector2(p.position.X + board_offset_x, p.position.Y + board_offset_y), null, new Color(255,255,255,transparency), p.obrot,Vector2.Zero, p.curent_size, SpriteEffects.None, 0f);
            else
            {
                _spriteBatch.Draw(p.picture, new Vector2(p.position.X + board_offset_x, p.position.Y + board_offset_y), null, new Color(255,255,255,transparency),   p.obrot, new Vector2(p.picture.Width / 2, p.picture.Height / 2), p.curent_size, SpriteEffects.None, 0f);
            }
        }
        public void klikniecie_przycisku_restart()
        {
            Initialize_Pieces();
            historia = new List<Move>();
            czyja_kolej = Kolor_figury.WHITE;
            block_mouse = false;
            koniec_gry = false;
            numer_obrazka = 0;
        }

        public bool czy_koniec_gry()
        {
            if (wylaczyc_kolejnosc) return false;
            Board b = new Board(figury, historia);
            int i_krola = -1;
            int j_krola = -1;
            List<Move> ruchy = new List<Move>();
            for(int i=0; i<8; i++)
            {
                for(int j=0; j<8; j++)
                {
                    if (b.plansza[i, j]!=null && b.plansza[i,j].kolor==czyja_kolej)
                    {
                        b.plansza[i, j].Moves(b,i,j,ruchy,true);
                        if (ruchy.Count != 0) return false;
                        
                           
                    }
                    if(b.plansza[i, j] != null && b.plansza[i, j].kolor == czyja_kolej && b.plansza[i, j].GetPoints()==10000)
                    {
                        i_krola = i;
                        j_krola = j;
                    }
                }
            }
            bool czy_pat=!b.czy_atak_na_pole(i_krola, j_krola, (czyja_kolej == Kolor_figury.WHITE) ? Kolor_figury.BLACK : Kolor_figury.WHITE);

            if (czy_pat)
            {
                MediaPlayer.Play(laugh);
                krol_matowany = figury.First(x => x.position == Move.IntToVector(new int[] { i_krola, j_krola }));
                krol_matowany.picture = Content.Load<Texture2D>((czyja_kolej == Kolor_figury.WHITE) ? "wKP" : "bKP");
                mat = false;
            }
            else // mat 
            {
                krol_matowany=figury.First(x => x.position == Move.IntToVector(new int[] { i_krola, j_krola }));
                MediaPlayer.Play(promotion_sound);
            }
                
            block_mouse = true;
            return true;
        }

        public void animacja_konca_gry()
        {
            
            if (numer_obrazka <animation_end_speed*8+1 ) numer_obrazka++;
            if(numer_obrazka%animation_end_speed==0 && mat)
                krol_matowany.picture = Content.Load<Texture2D>($"bK{numer_obrazka/animation_end_speed}");
        }



        // MessageBox - Import the Windows API MessageBox function
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int MessageBox(IntPtr hWnd, String text, String caption, uint type);

        public void ShowMessageBox(string text, string caption)
        {
            MessageBox(IntPtr.Zero, text, caption, 0);
        }

    }

}