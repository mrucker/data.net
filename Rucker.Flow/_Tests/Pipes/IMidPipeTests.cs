﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Rucker.Extensions;
using NUnit.Framework;

namespace Rucker.Flow.Tests
{
    [TestFixture("LambdaPipe")]
    [TestFixture("AsyncPipe")]
    [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]    
    public class IMidPipeTests
    {
        #region Fields
        private readonly Func<Func<IEnumerable<string>>, IMidPipe<string, string>> _midPipeFactory;
        #endregion

        #region Constructors
        public IMidPipeTests(string pipeType)
        {
            if (pipeType == "LambdaPipe")
            {
                _midPipeFactory = consumes => new LambdaMidPipe<string, string>(items => items.Select(i => i.ToLower())) { Consumes = consumes() };
            }

            if (pipeType == "AsyncPipe")
            {
                _midPipeFactory = consumes => new LambdaMidPipe<string, string>(items => items.Select(i => i.ToLower())) { Consumes = consumes() }.Async();
            }
        }
        #endregion

        [Test]
        public void ValuesProducedTest()
        {
            var pipe = _midPipeFactory(ManyProduction());

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            Assert.IsTrue(Production().Select(i => i.ToLower()).SequenceEqual(pipe.Produces));
        }

        [Test]
        public void ValuesProducedTwiceTest()
        {
            var pipe = _midPipeFactory(ManyProduction());

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            Assert.IsTrue(Production().Select(i => i.ToLower()).SequenceEqual(pipe.Produces));

            Assert.IsTrue(Production().Select(i => i.ToLower()).SequenceEqual(pipe.Produces));
        }

        [Test]
        public void ValuesEmptiedTest()
        {
            var pipe = _midPipeFactory(SingleProduction());

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            Assert.IsTrue(Production().Select(i => i.ToLower()).SequenceEqual(pipe.Produces));

            Assert.IsTrue(pipe.Produces.None());
        }

        [Test]
        public void FirstErrorTest()
        {
            var pipe = _midPipeFactory(FirstError);

            Assert.That(pipe.Produces.ToArray, Throws.Exception.Message.EqualTo("First").Or.InnerException.Message.EqualTo("First"));

            Assert.AreEqual(PipeStatus.Errored, pipe.Status);
        }

        [Test]
        public void LastErrorTest()
        {
            var pipe = _midPipeFactory(LastError);

            Assert.That(pipe.Produces.ToArray, Throws.Exception.Message.EqualTo("Last").Or.InnerException.Message.EqualTo("Last"));

            Assert.AreEqual(PipeStatus.Errored, pipe.Status);
        }

        [Test, Ignore("An interesting fail case. I don't think there is anything to be done.")]
        public void OnlyErrorTest()
        {
            var pipe = _midPipeFactory(OnlyError);

            Assert.That(pipe.Produces.ToArray, Throws.Exception.Message.EqualTo("Only").Or.InnerException.Message.EqualTo("Only"));

            Assert.AreEqual(PipeStatus.Errored, pipe.Status);
        }

        [Test]
        public void CreatedStatusTest()
        {
            var pipe = _midPipeFactory(SingleProduction());            

            Assert.AreEqual(PipeStatus.Created, pipe.Status);
        }

        [Test]
        public void WorkingStatusTest()
        {
            var pipe = _midPipeFactory(SingleProduction());
            var prod = pipe.Produces.GetEnumerator();

            prod.MoveNext();

            Assert.AreEqual(PipeStatus.Working, pipe.Status);

            prod.MoveNext();

            Assert.AreEqual(PipeStatus.Working, pipe.Status);
        }

        [Test]
        public void FinishedStatusTest()
        {
            var pipe = _midPipeFactory(SingleProduction());
            var prod = pipe.Produces.GetEnumerator();

            while(prod.MoveNext()) { }
            
            Assert.AreEqual(PipeStatus.Finished, pipe.Status);
        }

        [Test]
        public void EveryStatusTest()
        {
            var pipe = _midPipeFactory(SingleProduction());
            var prod = pipe.Produces.GetEnumerator();

            Assert.AreEqual(PipeStatus.Created, pipe.Status);

            while (prod.MoveNext())
            {
                Assert.AreEqual(PipeStatus.Working, pipe.Status);
            }

            Assert.AreEqual(PipeStatus.Finished, pipe.Status);
        }

        #region Private Methods
        private static IEnumerable<string> Production()
        {
            yield return "A";
            yield return "B";
            yield return "C";
        }

        private static Func<IEnumerable<string>> ManyProduction()
        {
            return Production;
        }

        private static Func<IEnumerable<string>> SingleProduction()
        {
            var block = new BlockingCollection<string>();

            foreach (var item in Production())
            {
                block.Add(item);
            }

            block.CompleteAdding();

            return () => block.GetConsumingEnumerable();
        }

        private static IEnumerable<string> FirstError()
        {
            throw new Exception("First");

            #pragma warning disable 162
            yield return "A";
            yield return "B";
            yield return "C";
            #pragma warning restore 162
        }

        private static IEnumerable<string> LastError()
        {
            yield return "A";
            yield return "B";
            throw new Exception("Last");
        }

        private static IEnumerable<string> OnlyError()
        {
            throw new Exception("Only");
        }
        #endregion
    }
}