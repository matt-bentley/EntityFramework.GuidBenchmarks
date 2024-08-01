using EntityFramework.GuidBenchmarks.DomainEvents;
using EntityFramework.GuidBenchmarks.DomainEvents.Abstract;
using EntityFramework.GuidBenchmarks.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;

namespace EntityFramework.GuidBenchmarks
{
    public sealed class BenchmarkService : BackgroundService
    {
        private readonly ILogger<BenchmarkService> _logger;
        private readonly Random _randomGenerator = new Random();

        public BenchmarkService(ILogger<BenchmarkService> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting benchmarks..");

            int count = 6_000_000;
            int queryBatchSize = 5;
            int maxConcurrency = 12;
            int logBatchSize = 1_000_000;
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = maxConcurrency
            };
            var random = new Random();

            // start benchmark
            var eventStreamType = typeof(BaselineEventStream);
            _logger.LogInformation("Running {type} benchmark for {i} items", eventStreamType.Name, count);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var ids = new ConcurrentQueue<Guid>();
            var baseLineIds = new ConcurrentQueue<int>();


            await Parallel.ForAsync(0, count, parallelOptions, async (i, _) =>
            {
                try
                {
                    var productId = Guid.NewGuid();
                    var events = GetEventsJson(productId);
                    var eventStream = new BaselineEventStream(events);

                    using (var dbContext = new AppDbContext())
                    {
                        dbContext.BaselineEventStreams.Add(eventStream);
                        await dbContext.SaveChangesAsync();
                        if ((i % queryBatchSize) == 0)
                        {
                            baseLineIds.Enqueue(eventStream.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inserting {type} {i}", i, eventStreamType.Name);
                }
                if (i % logBatchSize == 0)
                {
                    _logger.LogInformation("Inserted {i} {type}s", i, eventStreamType.Name);
                }
            });

            _logger.LogInformation("Inserted {count} {type} in: {elapsed}s", count, eventStreamType.Name, stopwatch.Elapsed.TotalSeconds);
            using (var dbContext = new AppDbContext())
            {
                dbContext.Statistics.Add(new Statistics(eventStreamType.Name, "Insert", count, stopwatch.Elapsed.TotalSeconds));
                await dbContext.SaveChangesAsync();
            }
            stopwatch.Restart();
            var queryCount = baseLineIds.Count;

            await Parallel.ForAsync(0, queryCount, parallelOptions, async (i, _) =>
            {
                try
                {
                    using (var dbContext = new AppDbContext())
                    {
                        if (baseLineIds.TryDequeue(out var id))
                        {
                            var inserted = await dbContext.BaselineEventStreams.FindAsync(id);
                            var events = DeserializeEvents(inserted.Events);
                            var updatedEvent = new ProductUpdatedEvent
                            {
                                ProductId = Guid.NewGuid(),
                                Name = inserted.Id + " Updated",
                                Price = GetRandomPrice()
                            };
                            events.Add(updatedEvent);
                            var eventsJson = SerializeEvents(events);
                            inserted.Update(eventsJson);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating {type} {i}", i, eventStreamType.Name);
                }
                if (i % logBatchSize == 0)
                {
                    _logger.LogInformation("Updated {i} {type}s", i, eventStreamType.Name);
                }
            });

            stopwatch.Stop();
            _logger.LogInformation("Updated {count} {type} in: {elapsed}s", queryCount, eventStreamType.Name, stopwatch.Elapsed.TotalSeconds);
            using (var dbContext = new AppDbContext())
            {
                dbContext.Statistics.Add(new Statistics(eventStreamType.Name, "Update", queryCount, stopwatch.Elapsed.TotalSeconds));
                await dbContext.SaveChangesAsync();
            }
            stopwatch.Reset();
            ids.Clear();


            // start benchmark
            eventStreamType = typeof(EventStream);
            _logger.LogInformation("Running {type} benchmark for {i} items", eventStreamType.Name, count);

            stopwatch.Start();

            ids = new ConcurrentQueue<Guid>();


            await Parallel.ForAsync(0, count, parallelOptions, async (i, _) =>
            {
                try
                {
                    var productId = Guid.NewGuid();
                    var events = GetEventsJson(productId);
                    var eventStream = new EventStream(productId, events);
                    using (var dbContext = new AppDbContext())
                    {
                        dbContext.EventStreams.Add(eventStream);
                        await dbContext.SaveChangesAsync();
                        if ((i % queryBatchSize) == 0)
                        {
                            ids.Enqueue(eventStream.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inserting {type} {i}", i, eventStreamType.Name);
                }
                if (i % logBatchSize == 0)
                {
                    _logger.LogInformation("Inserted {i} {type}s", i, eventStreamType.Name);
                }
            });
            _logger.LogInformation("Inserted {count} {type} in: {elapsed}s", count, eventStreamType.Name, stopwatch.Elapsed.TotalSeconds);
            using (var dbContext = new AppDbContext())
            {
                dbContext.Statistics.Add(new Statistics(eventStreamType.Name, "Insert", count, stopwatch.Elapsed.TotalSeconds));
                await dbContext.SaveChangesAsync();
            }
            stopwatch.Restart();
            queryCount = ids.Count;

            await Parallel.ForAsync(0, queryCount, parallelOptions, async (i, _) =>
            {
                try
                {
                    using (var dbContext = new AppDbContext())
                    {
                        if (ids.TryDequeue(out var id))
                        {
                            var inserted = await dbContext.EventStreams.FindAsync(id);
                            var events = DeserializeEvents(inserted.Events);
                            var updatedEvent = new ProductUpdatedEvent
                            {
                                ProductId = inserted.Id,
                                Name = inserted.Id + " Updated",
                                Price = GetRandomPrice()
                            };
                            events.Add(updatedEvent);
                            var eventsJson = SerializeEvents(events);
                            inserted.Update(eventsJson);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating {type} {i}", i, eventStreamType.Name);
                }
                if (i % logBatchSize == 0)
                {
                    _logger.LogInformation("Updated {i} {type}s", i, eventStreamType.Name);
                }
            });

            stopwatch.Stop();
            _logger.LogInformation("Updated {count} {type} in: {elapsed}s", queryCount, eventStreamType.Name, stopwatch.Elapsed.TotalSeconds);
            using (var dbContext = new AppDbContext())
            {
                dbContext.Statistics.Add(new Statistics(eventStreamType.Name, "Update", queryCount, stopwatch.Elapsed.TotalSeconds));
                await dbContext.SaveChangesAsync();
            }
            ids.Clear();


            // start benchmark
            eventStreamType = typeof(OrderedEventStream);
            _logger.LogInformation("Running {type} benchmark for {i} items", eventStreamType.Name, count);

            stopwatch.Start();

            ids = new ConcurrentQueue<Guid>();


            await Parallel.ForAsync(0, count, parallelOptions, async (i, _) =>
            {
                try
                {
                    var productId = OrderedEventStream.GenerateId();
                    var events = GetEventsJson(productId);
                    var eventStream = new OrderedEventStream(productId, events);
                    using (var dbContext = new AppDbContext())
                    {
                        dbContext.OrderedEventStreams.Add(eventStream);
                        await dbContext.SaveChangesAsync();
                        if ((i % queryBatchSize) == 0)
                        {
                            ids.Enqueue(eventStream.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inserting {type} {i}", i, eventStreamType.Name);
                }
                if (i % logBatchSize == 0)
                {
                    _logger.LogInformation("Inserted {i} {type}s", i, eventStreamType.Name);
                }
            });
            _logger.LogInformation("Inserted {count} {type} in: {elapsed}s", count, eventStreamType.Name, stopwatch.Elapsed.TotalSeconds);
            using (var dbContext = new AppDbContext())
            {
                dbContext.Statistics.Add(new Statistics(eventStreamType.Name, "Insert", count, stopwatch.Elapsed.TotalSeconds));
                await dbContext.SaveChangesAsync();
            }
            stopwatch.Restart();
            queryCount = ids.Count;

            await Parallel.ForAsync(0, queryCount, parallelOptions, async (i, _) =>
            {
                try
                {
                    using (var dbContext = new AppDbContext())
                    {
                        if (ids.TryDequeue(out var id))
                        {
                            var inserted = await dbContext.OrderedEventStreams.FindAsync(id);
                            var events = DeserializeEvents(inserted.Events);
                            var updatedEvent = new ProductUpdatedEvent
                            {
                                ProductId = inserted.Id,
                                Name = inserted.Id + " Updated",
                                Price = GetRandomPrice()
                            };
                            events.Add(updatedEvent);
                            var eventsJson = SerializeEvents(events);
                            inserted.Update(eventsJson);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating {type} {i}", i, eventStreamType.Name);
                }
                if (i % logBatchSize == 0)
                {
                    _logger.LogInformation("Updated {i} {type}s", i, eventStreamType.Name);
                }
            });

            stopwatch.Stop();
            _logger.LogInformation("Updated {count} {type} in: {elapsed}s", queryCount, eventStreamType.Name, stopwatch.Elapsed.TotalSeconds);
            using (var dbContext = new AppDbContext())
            {
                dbContext.Statistics.Add(new Statistics(eventStreamType.Name, "Update", queryCount, stopwatch.Elapsed.TotalSeconds));
                await dbContext.SaveChangesAsync();
            }
            ids.Clear();


            // start benchmark
            eventStreamType = typeof(OrderedCompressedEventStream);
            _logger.LogInformation("Running {type} benchmark for {i} items", eventStreamType.Name, count);

            stopwatch.Start();

            ids = new ConcurrentQueue<Guid>();


            await Parallel.ForAsync(0, count, parallelOptions, async (i, _) =>
            {
                try
                {
                    var productId = OrderedCompressedEventStream.GenerateId();
                    var events = GetEventsJson(productId);
                    var eventStream = new OrderedCompressedEventStream(productId, events);
                    using (var dbContext = new AppDbContext())
                    {
                        dbContext.OrderedCompressedEventStreams.Add(eventStream);
                        await dbContext.SaveChangesAsync();
                        if ((i % queryBatchSize) == 0)
                        {
                            ids.Enqueue(eventStream.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inserting {type} {i}", i, eventStreamType.Name);
                }
                if (i % logBatchSize == 0)
                {
                    _logger.LogInformation("Inserted {i} {type}s", i, eventStreamType.Name);
                }
            });
            _logger.LogInformation("Inserted {count} {type} in: {elapsed}s", count, eventStreamType.Name, stopwatch.Elapsed.TotalSeconds);
            using (var dbContext = new AppDbContext())
            {
                dbContext.Statistics.Add(new Statistics(eventStreamType.Name, "Insert", count, stopwatch.Elapsed.TotalSeconds));
                await dbContext.SaveChangesAsync();
            }
            stopwatch.Restart();
            queryCount = ids.Count;

            await Parallel.ForAsync(0, queryCount, parallelOptions, async (i, _) =>
            {
                try
                {
                    using (var dbContext = new AppDbContext())
                    {
                        if (ids.TryDequeue(out var id))
                        {
                            var inserted = await dbContext.OrderedCompressedEventStreams.FindAsync(id);
                            var events = DeserializeEvents(inserted.Events);
                            var updatedEvent = new ProductUpdatedEvent
                            {
                                ProductId = inserted.Id,
                                Name = inserted.Id + " Updated",
                                Price = GetRandomPrice()
                            };
                            events.Add(updatedEvent);
                            var eventsJson = SerializeEvents(events);
                            inserted.Update(eventsJson);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating {type} {i}", i, eventStreamType.Name);
                }
                if (i % logBatchSize == 0)
                {
                    _logger.LogInformation("Updated {i} {type}s", i, eventStreamType.Name);
                }
            });

            stopwatch.Stop();
            _logger.LogInformation("Updated {count} {type} in: {elapsed}s", queryCount, eventStreamType.Name, stopwatch.Elapsed.TotalSeconds);
            using (var dbContext = new AppDbContext())
            {
                dbContext.Statistics.Add(new Statistics(eventStreamType.Name, "Update", queryCount, stopwatch.Elapsed.TotalSeconds));
                await dbContext.SaveChangesAsync();
            }
            ids.Clear();


            // start benchmark
            eventStreamType = typeof(SequentialEventStream);
            _logger.LogInformation("Running {type} benchmark for {i} items", eventStreamType.Name, count);

            stopwatch.Start();

            ids = new ConcurrentQueue<Guid>();


            await Parallel.ForAsync(0, count, parallelOptions, async (i, _) =>
            {
                try
                {
                    var productId = Guid.NewGuid();
                    var events = GetEventsJson(productId);
                    var eventStream = new SequentialEventStream(productId, events);
                    using (var dbContext = new AppDbContext())
                    {
                        dbContext.SequentialEventStreams.Add(eventStream);
                        await dbContext.SaveChangesAsync();
                        if ((i % queryBatchSize) == 0)
                        {
                            ids.Enqueue(eventStream.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inserting {type} {i}", i, eventStreamType.Name);
                }
                if (i % logBatchSize == 0)
                {
                    _logger.LogInformation("Inserted {i} {type}s", i, eventStreamType.Name);
                }
            });
            _logger.LogInformation("Inserted {count} {type} in: {elapsed}s", count, eventStreamType.Name, stopwatch.Elapsed.TotalSeconds);
            using (var dbContext = new AppDbContext())
            {
                dbContext.Statistics.Add(new Statistics(eventStreamType.Name, "Insert", count, stopwatch.Elapsed.TotalSeconds));
                await dbContext.SaveChangesAsync();
            }
            stopwatch.Restart();
            queryCount = ids.Count;

            await Parallel.ForAsync(0, queryCount, parallelOptions, async (i, _) =>
            {
                try
                {
                    using (var dbContext = new AppDbContext())
                    {
                        if (ids.TryDequeue(out var id))
                        {
                            var inserted = await dbContext.SequentialEventStreams.FindAsync(id);
                            var events = DeserializeEvents(inserted.Events);
                            var updatedEvent = new ProductUpdatedEvent
                            {
                                ProductId = inserted.Id,
                                Name = inserted.Id + " Updated",
                                Price = GetRandomPrice()
                            };
                            events.Add(updatedEvent);
                            var eventsJson = SerializeEvents(events);
                            inserted.Update(eventsJson);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating {type} {i}", i, eventStreamType.Name);
                }
                if (i % logBatchSize == 0)
                {
                    _logger.LogInformation("Updated {i} {type}s", i, eventStreamType.Name);
                }
            });

            stopwatch.Stop();
            _logger.LogInformation("Updated {count} {type} in: {elapsed}s", queryCount, eventStreamType.Name, stopwatch.Elapsed.TotalSeconds);
            using (var dbContext = new AppDbContext())
            {
                dbContext.Statistics.Add(new Statistics(eventStreamType.Name, "Update", queryCount, stopwatch.Elapsed.TotalSeconds));
                await dbContext.SaveChangesAsync();
            }
            ids.Clear();


            // start benchmark
            eventStreamType = typeof(SequentialCompressedEventStream);
            _logger.LogInformation("Running {type} benchmark for {i} items", eventStreamType.Name, count);

            stopwatch.Start();

            ids = new ConcurrentQueue<Guid>();


            await Parallel.ForAsync(0, count, parallelOptions, async (i, _) =>
            {
                try
                {
                    var productId = Guid.NewGuid();
                    var events = GetEventsJson(productId);
                    var eventStream = new SequentialCompressedEventStream(productId, events);
                    using (var dbContext = new AppDbContext())
                    {
                        dbContext.SequentialCompressedEventStreams.Add(eventStream);
                        await dbContext.SaveChangesAsync();
                        if ((i % queryBatchSize) == 0)
                        {
                            ids.Enqueue(eventStream.Id);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error inserting {type} {i}", i, eventStreamType.Name);
                }
                if (i % logBatchSize == 0)
                {
                    _logger.LogInformation("Inserted {i} {type}s", i, eventStreamType.Name);
                }
            });
            _logger.LogInformation("Inserted {count} {type} in: {elapsed}s", count, eventStreamType.Name, stopwatch.Elapsed.TotalSeconds);
            using (var dbContext = new AppDbContext())
            {
                dbContext.Statistics.Add(new Statistics(eventStreamType.Name, "Insert", count, stopwatch.Elapsed.TotalSeconds));
                await dbContext.SaveChangesAsync();
            }
            stopwatch.Restart();
            queryCount = ids.Count;

            await Parallel.ForAsync(0, queryCount, parallelOptions, async (i, _) =>
            {
                try
                {
                    using (var dbContext = new AppDbContext())
                    {
                        if (ids.TryDequeue(out var id))
                        {
                            var inserted = await dbContext.SequentialCompressedEventStreams.FindAsync(id);
                            var events = DeserializeEvents(inserted.Events);
                            var updatedEvent = new ProductUpdatedEvent
                            {
                                ProductId = inserted.Id,
                                Name = inserted.Id + " Updated",
                                Price = GetRandomPrice()
                            };
                            events.Add(updatedEvent);
                            var eventsJson = SerializeEvents(events);
                            inserted.Update(eventsJson);
                            await dbContext.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating {type} {i}", i, eventStreamType.Name);
                }
                if (i % logBatchSize == 0)
                {
                    _logger.LogInformation("Updated {i} {type}s", i, eventStreamType.Name);
                }
            });

            stopwatch.Stop();
            _logger.LogInformation("Updated {count} {type} in: {elapsed}s", queryCount, eventStreamType.Name, stopwatch.Elapsed.TotalSeconds);
            using (var dbContext = new AppDbContext())
            {
                dbContext.Statistics.Add(new Statistics(eventStreamType.Name, "Update", queryCount, stopwatch.Elapsed.TotalSeconds));
                await dbContext.SaveChangesAsync();
            }
            ids.Clear();
        }

        private string GetEventsJson(Guid id)
        {
            var updateEventsCount = 10;
            var name = id.ToString();
            var createdEvent = new ProductCreatedEvent
            {
                ProductId = id,
                Name = name,
                Price = GetRandomPrice()
            };
            var events = new List<DomainEvent>(updateEventsCount + 1)
            {
                createdEvent
            };
            for(int i = 0; i < updateEventsCount; i++)
            {
                var updatedEvent = new ProductUpdatedEvent
                {
                    ProductId = id,
                    Name = name,
                    Price = GetRandomPrice()
                };
                events.Add(updatedEvent);
            }
            return SerializeEvents(events);
        }

        private static string SerializeEvents(List<DomainEvent> events)
        {
            return JsonSerializer.Serialize(events);
        }

        private static List<DomainEvent> DeserializeEvents(string json)
        {
            return JsonSerializer.Deserialize<List<DomainEvent>>(json);
        }

        private decimal GetRandomPrice()
        {
            var price = (decimal)_randomGenerator.Next(0, 1000);
            price += (decimal)_randomGenerator.NextDouble();
            return price;
        }
    }
}
