using System;
using System.Threading;

namespace Examples
{
    public partial class Program
    {
        // public static void Main(string[] args)
        // {
        //     // BankDemo11();
        //     // BankDemo12();
        //     // BankDemo13();
        //     // BankDemo14();
        //     // MonitorDemo();
        //     // MutexDemo();
        //     // RwLockDemo();
        //     // SemaphoreSlimDemo();
        //     AutoDeadLockDemo();
        //     // ThreadStaticDemo();
        //     // ThreadLocalDemo();
        // }

        private static void BankDemo11()
        {
            for (var i = 0; i < 42; i++)
            {
                var vasya = new BankAccount("Vasya") {Rubles = 100};            
                var petya = new BankAccount("Petya");
                var sasha = new BankAccount("Sasha");

                var thread1 = StartBckThread(() =>
                {
                    SendMoney(vasya, petya, 100);
                });
                
                var thread2 = StartBckThread(() =>
                {
                    SendMoney(vasya, sasha, 100);
                });

                thread1.Join();
                thread2.Join();

                Console.WriteLine($"ATTEMPT #{i:00}: {vasya}, {petya}, {sasha}");
            }
        }

        private static void BankDemo12()
        {
            for (var i = 0; i < 42; i++)
            {
                var vasya = new BankAccount("Vasya") {Rubles = 100};            
                var petya = new BankAccount("Petya");
                var sasha = new BankAccount("Sasha");

                var thread1 = StartBckThread(() =>
                {
                    lock (vasya)
                    {
                        lock (petya)
                        {
                            SendMoney(vasya, petya, 100);
                        }
                    }
                });
                
                var thread2 = StartBckThread(() =>
                {
                    lock (vasya)
                    {
                        lock (sasha)
                        {
                            SendMoney(vasya, sasha, 100);
                        }
                    }
                });

                thread1.Join();
                thread2.Join();

                Console.WriteLine($"ATTEMPT #{i:00}: {vasya}, {petya}, {sasha}");
            }
        }

        private static void BankDemo13()
        {
            for (var i = 0; i < 42; i++)
            {
                var vasya = new BankAccount("Vasya") {Rubles = 100};
                var petya = new BankAccount("Petya") {Rubles = 100};

                var thread1 = StartBckThread(() =>
                {
                    lock (vasya)
                    {
                        Thread.Sleep(new Random().Next(0, 100));
                        lock (petya)
                        {
                            SendMoney(vasya, petya, 10);
                        }
                    }
                });
                
                var thread2 = StartBckThread(() =>
                {
                    lock (vasya)
                    {
                        Thread.Sleep(new Random().Next(0, 100));
                        lock (petya)
                        {
                            SendMoney(petya, vasya, 20);
                        }
                    }
                });

                thread1.Join();
                thread2.Join();

                Console.WriteLine($"ATTEMPT #{i:00}: {vasya}, {petya}");
            }
        }

        private static void BankDemo14()
        {
            for (var i = 0; i < 42; i++)
            {
                var vasya = new BankAccount("Vasya") {Rubles = 100};
                var petya = new BankAccount("Petya") {Rubles = 100};

                var thread1 = StartBckThread(() =>
                {
                    lock (vasya)
                    {
                        Thread.Sleep(new Random().Next(0, 100));
                        lock (petya)
                        {
                            SendMoney(vasya, petya, 10);
                        }
                    }
                });
                
                var thread2 = StartBckThread(() =>
                {
                    lock (vasya)
                    {
                        Thread.Sleep(new Random().Next(0, 100));
                        lock (petya)
                        {
                            SendMoney(petya, vasya, 20);
                        }
                    }
                });

                thread1.Join();
                thread2.Join();

                Console.WriteLine($"ATTEMPT #{i:00}: {vasya}, {petya}");
            }
        }

        private static void MonitorDemo()
        {
            var obj = new object();

            lock (obj)
            {
                SendMoney(null, null, 0);
            }

            var lockTaken = false;
            try
            {
                Monitor.Enter(obj, ref lockTaken);
                
                SendMoney(null, null, 0);
            }
            finally
            {
                if (lockTaken)
                    Monitor.Exit(obj);
            }
        }
        
        private static void MutexDemo()
        {
            var mutex = new Mutex(false, "MyAwesomeProgramName");

            var isTaken = false;
            try
            {
                isTaken = mutex.WaitOne(TimeSpan.FromSeconds(1));
                if (isTaken)
                {
                    Console.WriteLine("It's first app instance. Press any key to exit ...");
                    Console.ReadKey();
                }
                else
                {
                    Console.WriteLine("Another instance of this program already running");
                }
            }
            finally
            {
                if (isTaken)
                    mutex.ReleaseMutex();
            }
        }

        private static void RwLockDemo()
        {
            using var rwLock = new ReaderWriterLockSlim();

            var t1 = StartBckThread(() =>
            {
                while (true)
                {
                    try
                    {
                        rwLock.EnterReadLock();
                        Console.WriteLine($"R1 Begin read");
                        Thread.Sleep(100);
                    }
                    finally
                    {
                        Console.WriteLine($"R1 End read");
                        rwLock.ExitReadLock();
                    }
                }
            });
            
            var t2 = StartBckThread(() =>
            {
                while (true)
                {
                    try
                    {
                        rwLock.EnterReadLock();
                        Console.WriteLine($"R2 Begin read");
                        Thread.Sleep(142);
                    }
                    finally
                    {
                        Console.WriteLine($"R2 End read");
                        rwLock.ExitReadLock();
                    }
                }
            });

            var t3 = StartBckThread(() =>
            {
                while (true)
                {
                    Thread.Sleep(300);
            
                    try
                    {
                        rwLock.EnterWriteLock();
                        Console.WriteLine($"W1 Begin write");                        
                        Thread.Sleep(150);
                    }
                    finally
                    {
                        Console.WriteLine($"W1 End write");
                        rwLock.ExitWriteLock();
                    }
                }
            });
            
            Thread.Sleep(TimeSpan.FromSeconds(10));
        }

        private static void SemaphoreSlimDemo()
        {
            using var semaphore = new SemaphoreSlim(2);
            
            void Foo()
            {
                while (true)
                {
                    try
                    {
                        semaphore.Wait();
                        Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: Enter");
                        Thread.Sleep(100);
                    }
                    finally
                    {
                        Console.WriteLine($"{Thread.CurrentThread.ManagedThreadId}: Exit");
                        semaphore.Release();
                    }
                }
            }

            var t1 = StartBckThread(Foo);
            var t2 = StartBckThread(Foo);
            var t3 = StartBckThread(Foo);

            Thread.Sleep(TimeSpan.FromSeconds(10));
        }

        private static void AutoDeadLockDemo()
        {
            using var semaphore = new SemaphoreSlim(1);

            Console.WriteLine(0);
            semaphore.Wait();
            Console.WriteLine(1);
            semaphore.Wait();
            Console.WriteLine(2);
            semaphore.Release();
            Console.WriteLine(3);
            semaphore.Release();
            Console.WriteLine(4);
        }

        [ThreadStatic] private static int staticVar = 0;
        
        private static void ThreadStaticDemo()
        {
            var localVar = 0;

            var t1 = StartBckThread(() =>
            {
                Console.WriteLine($"Thread#{Thread.CurrentThread.ManagedThreadId} a = {staticVar}, b = {localVar}");
                Console.WriteLine($"Thread#{Thread.CurrentThread.ManagedThreadId} Set a to 1, set b to 1");
                staticVar = 1;
                localVar = 1;
                Console.WriteLine($"Thread#{Thread.CurrentThread.ManagedThreadId} a = {staticVar}, b = {localVar}");
                Thread.Sleep(1000);
                Console.WriteLine($"Thread#{Thread.CurrentThread.ManagedThreadId} a = {staticVar}, b = {localVar}");
            });

            var t2 = StartBckThread(() =>
            {
                Thread.Sleep(100);
                Console.WriteLine($"Thread#{Thread.CurrentThread.ManagedThreadId} a = {staticVar}, b = {localVar}");
                Console.WriteLine($"Thread#{Thread.CurrentThread.ManagedThreadId} Set a to 2, set b to 2");
                staticVar = 2;
                localVar = 2;
                Console.WriteLine($"Thread#{Thread.CurrentThread.ManagedThreadId} a = {staticVar}, b = {localVar}");
            });

            t1.Join();
            t2.Join();
        }
        
        private static void ThreadLocalDemo()
        {
            using var threadLocal = new ThreadLocal<int>();
            var localVar = 0;

            var t1 = StartBckThread(() =>
            {
                Console.WriteLine($"Thread#{Thread.CurrentThread.ManagedThreadId} a = {threadLocal.Value}, b = {localVar}");
                Console.WriteLine($"Thread#{Thread.CurrentThread.ManagedThreadId} Set a to 1, set b to 1");
                threadLocal.Value = 1;
                localVar = 1;
                Console.WriteLine($"Thread#{Thread.CurrentThread.ManagedThreadId} a = {threadLocal.Value}, b = {localVar}");
                Thread.Sleep(1000);
                Console.WriteLine($"Thread#{Thread.CurrentThread.ManagedThreadId} a = {threadLocal.Value}, b = {localVar}");
            });

            var t2 = StartBckThread(() =>
            {
                Thread.Sleep(100);
                Console.WriteLine($"Thread#{Thread.CurrentThread.ManagedThreadId} a = {threadLocal.Value}, b = {localVar}");
                Console.WriteLine($"Thread#{Thread.CurrentThread.ManagedThreadId} Set a to 2, set b to 2");
                threadLocal.Value = 2;
                localVar = 2;
                Console.WriteLine($"Thread#{Thread.CurrentThread.ManagedThreadId} a = {threadLocal.Value}, b = {localVar}");
            });

            t1.Join();
            t2.Join();
        }
        
        private static void SendMoney(BankAccount from, BankAccount to, long value)
        {
            if (from.Rubles >= value)
            {
                Thread.Sleep(new Random().Next(0, 100));
                from.Rubles -= value;
                to.Rubles += value;
                Console.WriteLine($"Transaction completed: {from.OwnerName} -> {to.OwnerName}: {value}");
            }
            else
            {
                Console.WriteLine($"Transaction rejected: {from.OwnerName}, нужно больше золота!");
            }
        }
    }
}