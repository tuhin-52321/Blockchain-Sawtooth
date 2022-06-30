using System.Collections.Concurrent;

namespace ParallellJobs
{
    public class ParallelTasksExecutor
    {
        private ConcurrentQueue<Action>[] _queueCommandsList;
        private Task[] _queueProcessorTasks;
        private AutoResetEvent[] _triggers;
        private bool _isRunning;

        private static int index = 0;

        public ParallelTasksExecutor(int noQueues)
        {
            _queueCommandsList = new ConcurrentQueue<Action>[noQueues];
            _queueProcessorTasks = new Task[noQueues]; 

            _triggers = new AutoResetEvent[noQueues]; 

            _isRunning = true;

            for(int i=0; i < noQueues; i++)
            {
                int j = i;
                _queueCommandsList[i] = new ConcurrentQueue<Action>();
                _queueProcessorTasks[i] = new Task(() => ProcessQueue(j));
                _triggers[i] = new AutoResetEvent(false);
                _queueProcessorTasks[i].Start();
            }

            
        }

        private void ProcessQueue(int queueIndex)
        {
            while (_isRunning)
            {
                if (_isRunning && _queueCommandsList[queueIndex].Count != 0)
                {
                    if (_queueCommandsList[queueIndex].TryDequeue(out Action? action))
                    {
                        if (action != null)
                        {
                            try
                            {
                                Console.Out.WriteLine($"[{queueIndex}] excuting job ... ");
                                action();
                            }
                            catch (Exception e)
                            {
                                Console.Out.WriteLine(e.ToString());
                                Console.Out.WriteLine(e.StackTrace);
                            }
                        }
                    }
                }

                // you wanna wait here, but only if there's nothing new to do
                if (_isRunning && _queueCommandsList[queueIndex].Count == 0)
                {
                    _triggers[queueIndex].WaitOne(10000, false);
                }
            }
        }

        public void Pause()
        {
            _isRunning = false;
        }
        public void Resume() 
        { 
            _isRunning = true; 
        }

        public bool AddTask(Action task)
        {
            if (_isRunning)
            {
                _queueCommandsList[index].Enqueue(task);
                _triggers[index].Set();

                index++;
                if(index == _triggers.Count())
                {
                    index = 0;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

    }


    class TestClass
    {
        static void Main(string[] args)
        {
            ParallelTasksExecutor executor = new ParallelTasksExecutor(1);

            for (int i = 0; i < 23; i++) {
                int j = i;
                executor.AddTask(() =>
                {
                    Thread.Sleep(j*1000);
                    Console.Out.WriteLine("Job " + j + " done.");
    
                });
            }

            while(true);
        }
    }

}

