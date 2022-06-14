import sys
import math
import random

## Constants
# Bases
BASE_X_TOP = 0
BASE_Y_TOP = 0
BASE_X_BOTTOM = 18000
BASE_Y_BOTTOM = 9000

# Placing my base and the opponent base
base_x, base_y = [int(i) for i in input().split()]
opp_base_x = BASE_X_BOTTOM - base_x
opp_base_y = BASE_Y_BOTTOM - base_y

# Types
TYPE_MONSTER = 0
TYPE_MY_HEROES = 1
TYPE_OPP_HEROES = 2

## Functions
# Distances
def distance(xh, yh, xm, ym):
    return math.hypot(xh - xm, yh - ym)

# Random_x to get mana
def rand_x():
    return random.randint(6000, 11000)
# Random_y to get mana
def rand_y():
    return random.randint(1000, 7000)


heroes_per_player = int(input())  # Always 3

# game loop
while True:
    my_health, my_mana = [int(j) for j in input().split()]
    opp_health, opp_mana = [int(j) for j in input().split()]
    entity_count = int(input())  # Amount of heros and monsters you can see

    monsters = []
    my_heroes = []
    opp_heroes = []
        
    for i in range(entity_count):
        id, _type, x, y, shield_life, is_controlled, health, vx, vy, near_base, threat_for = [int(j) for j in input().split()]

        entity = {
            'id': id,
            'type': type,
            'x': x,
            'y': y,
            'shield_life': shield_life,
            'is_controlled': is_controlled,
            'health': health,
            'vx': vx,
            'vy': vy,
            'near_base': near_base,
            'threat_for': threat_for,
        }

        if _type == TYPE_MONSTER:
            monsters.append(entity)

        elif _type == TYPE_MY_HEROES:
            my_heroes.append(entity)

        elif _type == TYPE_OPP_HEROES:
            opp_heroes.append(entity)

        else:
            False


    # Attribute the level of threaten for monsters 
    monsters_threat_my_base = []
    monsters_threat_opp_base = []
    random_monsters = []
    for monster in monsters:
        if monster['threat_for'] == 1:
            if monster['near_base'] == 1:
                threat_level = 100
            else: 
                threat_level = 50
            dist = distance(base_x, base_y, monster['x'], monster['y'])
            threat_level += 50 * (1/dist + 1)
            monsters_threat_my_base.append((threat_level, monster))
        elif monster['threat_for'] == 2:
            if monster['near_base'] == 1:
                 threat_level = 100
            else: 
                threat_level = 50
            dist = distance(opp_base_x, opp_base_y, monster['x'], monster['y'])
            threat_level += 50 * (1/dist + 1)
            monsters_threat_opp_base.append((threat_level, monster))
        else: 
            random_monsters.append(monster)
                
    monsters_threat_my_base.sort(reverse=True)
    monsters_threat_opp_base.sort(reverse=True)


    
    # Manage the behaviour of my heroes
    for my_hero in my_heroes:

        # First hero in defense
        if my_hero['id']==0: 
            if len(monsters_threat_my_base)>my_hero['id']:
                target = monsters_threat_my_base[my_hero['id']][1]
                dist = distance(my_hero['x'], my_hero['y'], target['x'], target['y'])
                monster_dist = distance(base_x, base_y, target['x'], target['y'])
                if my_mana>10:
                    if (len(monsters_threat_my_base)>3 or monster_dist<1000) and dist<1280 and target['shield_life']==0:
                        print('SPELL WIND', opp_base_x, opp_base_y, my_hero['id'])
                    else:
                        print('MOVE', target['x'], target['y'], my_hero['id'])
                else: 
                    print('MOVE', target['x'], target['y'], my_hero['id'])
            else:
                print('MOVE', abs(base_x - 5000), abs(base_y - 1750), my_hero['id'])

        # Second hero control monster to opposant base
        if my_hero['id']==1: 
            if len(monsters_threat_my_base)>my_hero['id']:
                target = monsters_threat_my_base[my_hero['id']][1]
                dist = distance(my_hero['x'], my_hero['y'], target['x'], target['y'])
                monster_dist = distance(base_x, base_y, target['x'], target['y'])
                if my_mana>10:
                    if (len(monsters_threat_my_base)>3 or monster_dist<1000) and dist<1280 and target['shield_life']==0:
                        print('SPELL WIND', opp_base_x, opp_base_y, my_hero['id'])
                    else:
                        print('MOVE', target['x'], target['y'], my_hero['id'])
                else: 
                    print('MOVE', target['x'], target['y'], my_hero['id'])
            else:
                print('MOVE', abs(base_x - 1750), abs(base_y - 5000), my_hero['id'])


        # Third hero attacking directly the opposant base
        elif my_hero['id']==2:
            monster_near_opp_base = False
            should_wind = False
            should_shield = False
            if monsters_threat_opp_base:
                for monster in monsters_threat_opp_base:
                    dist = distance(opp_base_x, opp_base_y, monster[1]['x'], monster[1]['y'])
                    
                    # Is the monster close to the opposant base
                    if dist<6000:
                        monster_near_opp_base = True
                        target = monster[1]
                        # Should i cast shield?
                        if dist<4000 and target['shield_life'] == 0:
                            should_shield = True

            if monster_near_opp_base:
                dist = distance(my_hero['x'], my_hero['y'], target['x'], target['y'])
                if dist<1280 and my_mana>30:
                    if should_shield:
                        print('SPELL SHIELD', target['id'], my_hero['id'])
                    else:
                        print('SPELL WIND', opp_base_x, opp_base_y, my_hero['id'])
                else:
                    print('MOVE', target['x'], target['y'], my_hero['id'])
            
            else:
                print('MOVE', abs(opp_base_x - 3500), abs(opp_base_y - 3500), my_hero['id'])
              