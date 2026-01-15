import pandas as pd
import pyodbc
import numpy as np
import os
import pyodbc
from dotenv import load_dotenv


df = pd.read_csv('C:/Users/Keenan/Desktop/Projects/mlb-agent/data/updatedpitching_2023-2025.csv')


def ip_to_outs(ip):
    if ip is None or ip=="":
        return None
    ip = str(ip)
    if "." in ip:
        inning, outs= ip.split(".")
        return int(inning)*3+int(outs)
    else:
        return int(ip)*3


#print(df)

team_df = df[['teamId','teamName']].drop_duplicates(subset=['teamId'])
#holds all players that played in any MLB season from 2023-2025
players_df = df[['mlbamId','fullName']].drop_duplicates(subset=['mlbamId'])
#df holds records for each player and what teams they played on in each season (multiple records for 1 season if player played for multiple teams)
player_team_season = df[['mlbamId','season','teamId']].drop_duplicates()
#drop duplicates within stats df to ensure no records are duplicated due to trades
player_stats_df=df.drop(columns=['fullName','teamName','teamId']).drop_duplicates(subset=['mlbamId','season'])

player_stats_df['outsPitched']=player_stats_df['ip'].apply(ip_to_outs)
player_stats_df['ip']=player_stats_df['ip'].astype(str)
player_stats_df["era"]  = pd.to_numeric(player_stats_df["era"], errors="coerce")
player_stats_df["whip"] = pd.to_numeric(player_stats_df["whip"], errors="coerce")
#select rows where outsPitched==0 then select cols era and whip
player_stats_df.loc[player_stats_df['outsPitched']==0,['era','whip']]=None
player_stats_df = player_stats_df.replace({np.nan: None})

print(players_df)
print(team_df)
print(player_team_season)
print(player_stats_df)


player_team_season["teamId"] = player_team_season["teamId"].astype(int)
print(player_team_season["teamId"].isna().sum())
print(player_stats_df.dtypes)
#connect to azure database

load_dotenv()
server = os.getenv("AZURE_SQL_SERVER")
database = os.getenv("AZURE_SQL_DATABASE")
username = os.getenv("AZURE_SQL_USER")
password = os.getenv("AZURE_SQL_PASSWORD")


conn = pyodbc.connect('DRIVER={ODBC Driver 18 for SQL Server};' \
f'SERVER={server};' \
f'DATABASE={database};' \
f'UID={username};' \
f'PWD={password};'
"Encrypt=yes;"
)

#open connection to DB
cursor = conn.cursor()

#cursor.executemany(
    #"#INSERT INTO dbo.Teams (teamId, teamName) VALUES (?, ?)",
   # team_df.itertuples(index=False, name=None)
#)
#conn.commit()
"""
cursor.executemany(
    "INSERT INTO dbo.Players (mlbamId, fullName) VALUES (?,?)",
    players_df.itertuples(index=False, name=None)
)
conn.commit()

cursor.executemany(
    "INSERT INTO dbo.PlayerSeasonTeams (mlbamId, season, teamId) VALUES (?,?,?)",
    player_team_season.itertuples(index=False, name=None)
)
conn.commit()
"""



cursor.executemany(
    "INSERT INTO dbo.PitchingStats (mlbamId, season, gamesPlayed, gamesStarted,ip,era,whip,so,bb,hr,w,l,saves,outsPitched) VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?)",
    player_stats_df.itertuples(index=False, name=None)
)

conn.commit()

