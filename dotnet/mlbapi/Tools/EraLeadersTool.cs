using OpenAI.Chat;

namespace mlbapi.Tools;

public static class EraLeadersTool
{
    public static ChatTool Create() =>
        ChatTool.CreateFunctionTool(
            functionName: "get_era_leaders",
            functionDescription:"Get the top pitchers by ERA in a given season",
            functionParameters: BinaryData.FromBytes("""
            {
                "type": "object",
                "properties": {
                    "season" : {"type": "integer", "descripton": "The year of the season e.g. 2024"},
                    "take" : {"type": "integer", "descripton": "The number of top pitchers to display"},
                    "outsPitched":{"type": "integer", "descripton": "Minimum number of outs pitched e.g. 9 outs pitched is 3 innings pitched"}
                },
                "required": ["season"]
            }

            """u8.ToArray())
        );

}

