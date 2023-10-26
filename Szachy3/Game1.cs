using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;

namespace Szachy3
{
    public class Game1 : Game
    {
        public enum Kolor_figury { WHITE, BLACK};
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
        }


        // ustawienia
        private const int window_width = 1300;
        private const int window_height = 900;


        // board
        Texture2D board;
        Vector2 board_position;
        int board_start_position_x = 0;
        int board_start_position_y = 0;
        public static int board_offset_x = 50;
        public static int board_offset_y = 50;
        public int zbite_biale=0;
        public int zbite_czarne=0;

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

        // button
        Texture2D button1;
        Vector2 button1_position;
        int button1_start_position_x = 1000;
        int button1_start_position_y = 350;

        // animation     
        static float targetScale = 0.3f;          // Target scale 
        static float animationDuration = 0.2f;    // Animation duration in seconds
        static float moveSpeed = 5000.0f;

        Piece pionek;

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
            Initialize_Sounds();
            Initialize_Board();
            Initialize_Pieces();
            Initialize_Buttons();
 
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }
     
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) //wychodznie klawiszami z aplikacji
                Exit();
            

            MouseState mouseState = Mouse.GetState();

            // wyswietlanie kursora raczki nad figura
            Mouse.SetCursor(figury.Any(figura => figura.IsMouseOver(mouseState)) ? MouseCursor.Hand : MouseCursor.Arrow);

            // lapanie figur
            lapanie_myszka_figur(mouseState);

            // klikanie przycisku - resetuj
            if (mouseState.LeftButton == ButtonState.Pressed && (new Rectangle((int)button1_position.X, (int)button1_position.Y, button1.Width, button1.Height)).Contains(mouseState.X, mouseState.Y))
                Initialize_Pieces();
            
            // Wlaczenie animacji zbicia - dla elementow ktore sa zbite
            figury.Where(x => x.zbity).ToList().ForEach(x => x.AnimacjaZbicia((float)gameTime.ElapsedGameTime.TotalSeconds));
            
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
            //rysuj_czerowne_pola(new Vector2[] {new Vector2(100,400), new Vector2(700, 100), new Vector2(700, 700) });
            
            // Rysowanie przycisku
            _spriteBatch.Draw(button1, button1_position, Color.White);
            
            // Rysowanie figur
            for (int i=0; i<liczba_figur; i++)
                rysuj_figure(figury[i]);
            
            // Rysowanie pionka trzymanego przez myszke na samej gorze
            if (index_dragged_piece != -1)
                rysuj_figure(figury[index_dragged_piece], 128);
           
            _spriteBatch.End();

            base.Draw(gameTime);
        }
        
        public void lapanie_myszka_figur(MouseState mouseState)
        {
            if (mouseState.LeftButton == ButtonState.Pressed) //klikniecie mysza
            {
                if (index_dragged_piece == -1) //jak jeszcze niczego nie trzyma
                {
                    // Wybieranie figury nad ktora jest myszka i ustawianie indexu i prev_position
                    var el = figury.Select((figura, index) => new {  Index = index, Figura = figura }).FirstOrDefault( x => x.Figura.IsMouseOver(mouseState));
                    if(el!=null)
                    {
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

                    // szukanie czy na polu postawionego pionka jest jakis inny
                    Piece zbita=figury.FirstOrDefault(x => x.position == figury[index_dragged_piece].position && x.id != figury[index_dragged_piece].id);
                    
                    if(zbita==null) // puste pole
                        MediaPlayer.Play(move_sound);
                    else //pole z figura
                    {
                        if (zbita.kolor == figury[index_dragged_piece].kolor) // pole z wlasna figura
                        {
                            figury[index_dragged_piece].position = prev_position; // powrot na poprzednie miejsce
                            MediaPlayer.Play(wrong_move_sound);
                        }
                        else // pole z figura przeciwnika
                            zbicie_przeciwnika(zbita);  
                    }
                    
                        
                }

                index_dragged_piece = -1;
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
        protected void Initialize_Pieces()
        {
            zbite_biale = 0;
            zbite_czarne = 0;
            liczba_figur = 32;

            figury = new Piece[liczba_figur];

            figury[0] = new Piece(Content.Load<Texture2D>("wR"));
            figury[1] = new Piece(Content.Load<Texture2D>("wN"));
            figury[2] = new Piece(Content.Load<Texture2D>("wB"));
            figury[3] = new Piece(Content.Load<Texture2D>("wQ"));
            figury[4] = new Piece(Content.Load<Texture2D>("wK"));
            figury[5] = new Piece(Content.Load<Texture2D>("wB"));
            figury[6] = new Piece(Content.Load<Texture2D>("wN"));
            figury[7] = new Piece(Content.Load<Texture2D>("wR"));

           

            for (int i = 8; i < 16; i++)
            {
                figury[i] = new Piece(Content.Load<Texture2D>("wP"));
            }

            figury[16] = new Piece(Content.Load<Texture2D>("bR"));
            figury[17] = new Piece(Content.Load<Texture2D>("bN"));
            figury[18] = new Piece(Content.Load<Texture2D>("bB"));
            figury[19] = new Piece(Content.Load<Texture2D>("bQ"));
            figury[20] = new Piece(Content.Load<Texture2D>("bK"));
            figury[21] = new Piece(Content.Load<Texture2D>("bB"));
            figury[22] = new Piece(Content.Load<Texture2D>("bN"));
            figury[23] = new Piece(Content.Load<Texture2D>("bR"));
            

            for (int i = 24; i < liczba_figur; i++)
            {
                figury[i] = new Piece(Content.Load<Texture2D>("bP"));
            }

            // Pozycje na planszy
            for (int i = 0; i < 16; i++)
                figury[i].position=new Vector2(100 * (i % 8), (((int)i / 8) + 1) % 2 * 100 + 600);
            
            for (int i = 16; i < liczba_figur; i++)
                figury[i].position= new Vector2(100 * (i % 8), ((int)(i / 8) - 2) * 100);
            

            for(int i=0; i<32; i++)
            {
                figury[i].kolor = (i < 16) ? Kolor_figury.WHITE : Kolor_figury.BLACK;
            }
        }
        protected void Initialize_Sounds()
        {
            this.move_sound = Content.Load<Song>("tech_sound");
            this.wrong_move_sound = Content.Load<Song>("sound2");
            this.capture_sound = Content.Load<Song>("explo_sound");
        }
        protected void Initialize_Buttons()
        {
            button1 = Content.Load<Texture2D>("resetuj");
            button1_position = new Vector2(button1_start_position_x, button1_start_position_y);
        }
        protected void rysuj_czerowne_pola(Vector2[] pola)
        {
            for(int i=0; i< pola.Length; i++) 
            {
                Texture2D czerwony = Content.Load<Texture2D>("czerwone");

                _spriteBatch.Draw(czerwony, new Vector2(pola[i].X + board_offset_x, pola[i].Y+board_offset_y), new Color(255, 255, 255, 128)); // 50% przezroczystości

            }
        }
        protected void rysuj_figure(Piece p, int transparency=255)
        {
            _spriteBatch.Draw(p.picture, new Vector2(p.position.X + board_offset_x, p.position.Y + board_offset_y), null, new Color(255,255,255,transparency), 0f, Vector2.Zero, p.curent_size, SpriteEffects.None, 0f);
        }
        
    }

}