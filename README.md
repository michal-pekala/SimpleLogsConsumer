# SimpleLogsConsumer
Single producer / multiple consumers app to import file into LiteDB

* producer - single thread to read input file, each line send to a queue as a new event
* events published on single instance of BlockingCollection<LoggedEvent> queue
* consumers - multiple threads that insert events to LiteDB (parameter)
* logs in input file are assumed to be stored in random order,
* no guarantee that for single event start/end lines are "close enough" to each other
* duration/alert information needs to be computed in context of both start and end lines
* for large files it can be too expensive to keep first event line in RAM until second apperars
* hence each logged line is inserted to db and then selected for final computation (two queries): 
** try find if second line is already inserted
** compute duration and update

Performance depends on files operation : imported logs file, LiteDB file.

Possible optimizations (for further profiling):
* fast bulk inserts to LiteDb / duration computed on sorted chunks of logs and stored in another collection
* keep logs file and db file on separated hard drives
* another tool for events queue (?)

BUILD:

MSBuild SimpleLogsConsumer.csproj /p:Configuration=Release

CLEAN:

MSBuild SimpleLogsConsumer.csproj /p:Configuration=Release -t:Clean

RUN:

SimpleLogsConsumer.exe path_to_log_files [consumers_count]
