namespace mlbapi.Models;

public record EraLeader(
    int mlbamId,
    string fullName,
    decimal era,
    int season
);