# SimpleLogsConsumer
Single producer / multiple consumers app to import file into LiteDB

BUILD:

MSBuild SimpleLogsConsumer.csproj /p:Configuration=Release

CLEAN:

MSBuild SimpleLogsConsumer.csproj /p:Configuration=Release -t:Clean

RUN:

SimpleLogsConsumer.exe path_to_log_files [consumers_count]
