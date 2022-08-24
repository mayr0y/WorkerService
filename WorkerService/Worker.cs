namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly object _lock = new();
        private readonly Mutex _mutex = new();
        private readonly Semaphore _semaphore = new(1, 1);

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                File.WriteAllText("ThreadLock", string.Empty);
                File.WriteAllText("ThreadMutex", string.Empty);
                File.WriteAllText("ThreadSemaphore", string.Empty);
                File.WriteAllText("TaskLock", string.Empty);
                File.WriteAllText("TaskMutex", string.Empty);
                File.WriteAllText("TaskSemaphore", string.Empty);

                ThreadLock();
                ThreadMutex();
                ThreadSemaphore();
                TaskLock();
                TaskMutex();
                TaskSemaphore();

                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }

        private void TaskSemaphore()
        {
            _semaphore.WaitOne();
            StartTask(TaskSemaphorePrint);
            _semaphore.Release();
        }

        private void TaskMutex()
        {
            _mutex.WaitOne();
            StartTask(TaskMutexPrint);
            _mutex.ReleaseMutex();
        }

        private void TaskLock() => StartTask(TaskLockPrint);

        private void TaskLockPrint()
        {
            lock (_lock)
            {
                CommonPrintTak("TaskLock");
            }
        }
        private void TaskMutexPrint() => CommonPrintTak("TaskMutex");
        private void TaskSemaphorePrint() => CommonPrintTak("TaskSemaphore");

        private void ThreadLock() => StartThread(ThreadLockPrint);

        private void ThreadLockPrint()
        {
            lock (_lock)
            {
                CommonPrintThread("ThreadLock");
            }
        }
        private void ThreadMutex() => StartThread(ThreadMutexPrint);

        private void ThreadMutexPrint()
        {
            _mutex.WaitOne();
            CommonPrintThread("ThreadMutex");
            _mutex.ReleaseMutex();
        }

        private void ThreadSemaphore() => StartThread(ThreadSemaphorePrint);

        private void ThreadSemaphorePrint()
        {
            _semaphore.WaitOne();
            CommonPrintThread("ThreadSemaphore");
            _semaphore.Release();
        }

        private static void CommonPrintThread(string nameTextFile)
        {
            var x = 1;
            for (int i = 0; i < 5; i++)
            {
                File.AppendAllText($"{nameTextFile}", $"CurrentThread - {Thread.CurrentThread.Name} : {x}\n");
                x++;
                Thread.Sleep(100);
            }
        }

        private static void CommonPrintTak(string nameTextFile)
        {
            var x = 1;
            for (int i = 0; i < 5; i++)
            {
                File.AppendAllText($"{nameTextFile}", $"CurrentTask - {Task.CurrentId} : {x}\n");
                x++;
                Thread.Sleep(100);
            }
        }

        private void StartThread(Action action)
        {
            for (int i = 0; i < 5; i++)
            {
                ThreadPool.QueueUserWorkItem(_ => action());
            }
        }

        private void StartTask(Action action)
        {
            for (int i = 0; i < 5; i++)
            {
                Task.Run(action);
            }
        }
    }
}