CREATE TABLE dbo.Players(
    mlbamId INT NOT NULL PRIMARY KEY,
    fullName NVARCHAR(100) NOT NULL
);

CREATE TABLE dbo.Teams (
    teamId INT NOT NULL PRIMARY KEY,
    teamName NVARCHAR(100) NOT NULL
);

CREATE TABLE dbo.PitchingStats(
    mlbamId INT NOT NULL,
    season INT NOT NULL,
    gamesPlayed INT NULL,
    gamesStarted INT NULL,
    ip VARCHAR(10) NULL,
    era DECIMAL(5,2) NULL,
    whip DECIMAL(6,3) NULL,
    so INT NULL,
    bb INT NULL,
    hr INT NULL,
    w INT NULL,
    l INT NULL,
    saves INT NULL,

    CONSTRAINT PK_PitchingStats PRIMARY KEY (mlbamId, season),

    CONSTRAINT FK_PS_Players FOREIGN KEY (mlbamId) REFERENCES dbo.Players(mlbamId),

   
    CHECK (gamesPlayed >= 0),
    CHECK (gamesStarted >= 0),
    CHECK (outsPitched >=0),
    CHECK (so >= 0),
    CHECK (bb >= 0),
    CHECK (hr >= 0),
    CHECK (w >= 0),
    CHECK (l >= 0),
    CHECK (saves >= 0),
    CHECK (era >= 0),
    CHECK (whip >= 0)



);

CREATE TABLE dbo.PlayerSeasonTeams(
    mlbamId INT NOT NULL,
    season INT NOT NULL,
    teamId INT NOT NULL,
    CONSTRAINT PK_PlayerSeasonTeams PRIMARY KEY (mlbamId, season, teamId),
    CONSTRAINT FK_PSTeams_players FOREIGN KEY (mlbamId) REFERENCES dbo.Players(mlbamId),
    
    CONSTRAINT FK_PSTeams_teams FOREIGN KEY (teamId) REFERENCES dbo.Teams(teamId)
)