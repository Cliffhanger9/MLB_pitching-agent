import statsapi
import pandas as pd
#s = statsapi.player_stat_data(player_id, group="pitching", type="season", season=season)

def get_season_block(player_id: int, season: int):
    s = statsapi.player_stat_data(player_id, group="pitching", type="season", season=season)
    block= s.get("stats",[])
    if not block:
        return None
    return block[0].get("stats",None)


rows=[]
seasons=[2023,2024,2025]
counter=0
for season in seasons:
    
    teams = statsapi.get('teams', {'sportId':1})['teams']
    for team in teams:
        team_id=team['id']
        roster = statsapi.get('team_roster',{'teamId':team_id, 'season':season})['roster']
        #import json

        #print(team["name"])
        #print(json.dumps(roster[0], indent=2)[:1500])
        

        #print(f"{team['name']} roster size: {len(roster)}")
        for player in roster:
            if player["position"]['abbreviation']!="P":
                continue
            player_id = player['person']['id']
            name = player['person']['fullName']
            position=player['position']['abbreviation']
            print(player_id, name,position)
        
            block = get_season_block(player_id, season)
            if not block:
                continue
            rows.append({
                'mlbamId' : player_id,
                'fullName' : name,
                'season':season,
                'teamId':team['id'],
                'teamName':team['name'],
                'gamesPlayed':block.get('gamesPlayed'),
                'gamesStarted':block.get('gamesStarted'),
                'ip':block.get('inningsPitched'),
                'era':block.get('era'),
                "whip":block.get('whip'),
                "so": block.get("strikeOuts"),
                "bb": block.get("baseOnBalls"),
                "hr": block.get("homeRuns"),
                'w':block.get('wins'),
                'l':block.get('losses'),
                'saves':block.get('saves'),

                })

df=pd.DataFrame(rows)
print(df.head)
print("Rows:", len(df))

df.to_csv("C:/Users/Keenan/Desktop/Projects/mlb-agent/data/updatedpitching_2023-2025.csv", index=False)
        
        

print(f"CSV written with {len(df)} rows")
