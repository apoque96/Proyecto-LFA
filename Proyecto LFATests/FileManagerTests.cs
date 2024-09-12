using Microsoft.VisualStudio.TestTools.UnitTesting;
using Proyecto_LFA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto_LFA.Tests
{
    [TestClass()]
    public class FileManagerTests
    {
        [TestMethod()]
        public void ValidGrammar()
        {
            Assert.IsInstanceOfType<(List<Set>, List<Token>, List<Action>)>
                (FileManager.ReadFile("../../../../Testing/GRAMATICA.txt"));
            Assert.IsInstanceOfType<(List<Set>, List<Token>, List<Action>)>
                (FileManager.ReadFile("../../../../Testing/prueba_2-1.txt"));
        }

        [TestMethod()]
        public void InvalidGrammar()
        {
            Assert.ThrowsException<ArgumentException>(
                () =>FileManager.ReadFile("../../../../Testing/prueba_1-1.txt"));
            Assert.ThrowsException<ArgumentException>(
                () => FileManager.ReadFile("../../../../Testing/prueba_3-1.txt"));
            Assert.ThrowsException<ArgumentException>(
                () => FileManager.ReadFile("../../../../Testing/prueba_4-1.txt"));
        }
    }
}