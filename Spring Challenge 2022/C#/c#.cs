using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    // Constants
    // Bases
    public const int BASE_X_TOP = 0;
    public const int BASE_Y_TOP = 0;
    public const int BASE_X_BOTTOM = 18000;
    public const int BASE_Y_BOTTOM = 9000;

    // Types
    public const int TYPE_MONSTER = 0;
    public const int TYPE_MY_HERO = 1;
    public const int TYPE_OP_HERO = 2;

    // Distance
    public const int MAX_RANGE_WIND = 1280;
    public const int ONE_STEP_DIST = 800;

    // Mana amounts
    public const int AMOUNT_MANA_NEEDED_ATTACK = 30;
    public const int AMOUNT_MANA_NEEDED_DEFEND = 10;

    public class Entity
    {
        public int Id;
        public int Type;
        public int X, Y;
        public int ShieldLife;
        public int IsControlled;
        public int Health;
        public int Vx, Vy;
        public int NearBase;
        public int ThreatFor;

        public Entity(int id, int type, int x, int y, int shieldLife, int isControlled, int health, int vx, int vy, int nearBase, int threatFor)
        {
            this.Id = id;
            this.Type = type;
            this.X = x;
            this.Y = y;
            this.ShieldLife = shieldLife;
            this.IsControlled = isControlled;
            this.Health = health;
            this.Vx = vx;
            this.Vy = vy;
            this.NearBase = nearBase;
            this.ThreatFor = threatFor;
        }
    }

    static void Main(string[] args)
    {
        string[] inputs;
        inputs = Console.ReadLine().Split(' ');

        // base_x,base_y: The corner of the map representing your base
        int baseX = int.Parse(inputs[0]);
        int baseY = int.Parse(inputs[1]);

        // opposant baseX and baseY
        int oppBaseX = BASE_X_BOTTOM - baseX;
        int oppBaseY = BASE_Y_BOTTOM - baseY;

        // heroesPerPlayer: Always 3
        int heroesPerPlayer = int.Parse(Console.ReadLine());

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int myHealth = int.Parse(inputs[0]); // Your base health
            int myMana = int.Parse(inputs[1]); // Ignore in the first league; Spend ten mana to cast a spell

            inputs = Console.ReadLine().Split(' ');
            int oppHealth = int.Parse(inputs[0]);
            int oppMana = int.Parse(inputs[1]);

            int entityCount = int.Parse(Console.ReadLine()); // Amount of heros and monsters you can see

            List<Entity> myHeroes = new List<Entity>(entityCount);
            List<Entity> oppHeroes = new List<Entity>(entityCount);
            List<Entity> monsters = new List<Entity>(entityCount);
            
            for (int i = 0; i < entityCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int id = int.Parse(inputs[0]); // Unique identifier
                int type = int.Parse(inputs[1]); // 0=monster, 1=your hero, 2=opponent hero
                int x = int.Parse(inputs[2]); // Position of this entity
                int y = int.Parse(inputs[3]);
                int shieldLife = int.Parse(inputs[4]); // Ignore for this league; Count down until shield spell fades
                int isControlled = int.Parse(inputs[5]); // Ignore for this league; Equals 1 when this entity is under a control spell
                int health = int.Parse(inputs[6]); // Remaining health of this monster
                int vx = int.Parse(inputs[7]); // Trajectory of this monster
                int vy = int.Parse(inputs[8]);
                int nearBase = int.Parse(inputs[9]); // 0=monster with no target yet, 1=monster targeting a base
                int threatFor = int.Parse(inputs[10]); // Given this monster's trajectory, is it a threat to 1=your base, 2=your opponent's base, 0=neither

                Entity entity = new Entity(
                    id, type, x, y, shieldLife, isControlled, health, vx, vy, nearBase, threatFor
                );

                switch (type)
                {
                    case TYPE_MONSTER:
                        monsters.Add(entity);
                        break;
                    case TYPE_MY_HERO:
                        myHeroes.Add(entity);
                        break;
                    case TYPE_OP_HERO:
                        oppHeroes.Add(entity);
                        break;
                }
            }

            List<Tuple<int, Entity>> monstersThreatRank = new List<Tuple<int, Entity>>(entityCount);
            for (int i=0; i<monsters.Count;i++)
            {
                int threatLevel = 0;
                
                if (monsters[i].NearBase == 1)
                {
                    threatLevel = 100;
                    if (monsters[i].ThreatFor == 1)
                    {
                        threatLevel += 50;
                    }
                    
                    int dist = Convert.ToInt32(Math.Sqrt(Math.Pow(baseX-monsters[i].X, 2) + Math.Pow(baseY-monsters[i].Y, 2)));
                    threatLevel += 50 * (1/dist+1);
                    monstersThreatRank.Add(Tuple.Create(threatLevel, monsters[i]));
                }
            }
            monstersThreatRank.Sort();

         
            // DEFENSE 
            // Number of heroes that defend
            
            for (int i = 0; i < heroesPerPlayer-1; i++)
            {
                if (monstersThreatRank.Count>i)
                {
                    Entity target = monstersThreatRank[0].Item2;

                    int dist = Convert.ToInt32(Math.Sqrt(Math.Pow(myHeroes[i].X-target.X, 2) + Math.Pow(myHeroes[i].Y-target.Y, 2)));
                    if (myMana>AMOUNT_MANA_NEEDED_DEFEND && dist<MAX_RANGE_WIND)
                    {
                        Console.WriteLine($"SPELL WIND {oppBaseX} {oppBaseY}");
                    }
                    else
                    {
                        Console.WriteLine($"MOVE {target.X} {target.Y}");
                    }
                }
                else
                {
                    Console.WriteLine($"MOVE {Math.Abs(baseX-3500)} {Math.Abs(baseY-3500)}");
                }
            }
            
            // ATTACK (only if my health is higher than 1hp)
            if (monsters.Count>0)
            {
                // Listing the monsters that could threat the opposant base
                List<Tuple<int, Entity>> monstersNearToOppBase = new List<Tuple<int, Entity>>(entityCount);
                for (int i = 0; i<monsters.Count; i++)
                {
                    int dist = Convert.ToInt32(Math.Sqrt(Math.Pow(oppBaseX-monsters[i].X, 2) + Math.Pow(oppBaseY-monsters[i].Y, 2)));
                    if (dist<7000)
                    {
                        monstersNearToOppBase.Add(Tuple.Create(dist, monsters[i]));
                    }
                }
                
                // Condition to focus a monster
                if (monstersNearToOppBase.Count>0)
                {
                    Entity target = monstersNearToOppBase[0].Item2; 

                    int dist = Convert.ToInt32(Math.Sqrt(Math.Pow(myHeroes[2].X-target.X, 2) + Math.Pow(myHeroes[2].Y-target.Y, 2)));
                    // Conditions to cast wind 
                    if ((myMana>AMOUNT_MANA_NEEDED_ATTACK) && (dist<MAX_RANGE_WIND))
                    {
                        Console.WriteLine($"SPELL WIND {oppBaseX} {oppBaseY}");
                    }
                    else
                    {
                        Console.WriteLine($"MOVE {target.X} {target.Y}");
                    }
                }
                else
                {
                    Console.WriteLine($"MOVE {oppBaseX - 3500} {oppBaseY - 3500}");
                }
            }
            else
            {
                Console.WriteLine($"MOVE {oppBaseX - 3500} {oppBaseY - 3500}");
            }
        }
    }
}