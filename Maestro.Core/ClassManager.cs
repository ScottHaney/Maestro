using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Maestro.Core
{
    public class ClassManager
    {
        private readonly InternalClassGraphGenerator _classGraphGenerator;
        private readonly InternalClassGraphAnalyzer _classGraphAnalyzer;

        public ClassManager(InternalClassGraphGenerator classGraphGenerator,
            InternalClassGraphAnalyzer classGraphAnalyzer)
        {
            _classGraphGenerator = classGraphGenerator;
            _classGraphAnalyzer = classGraphAnalyzer;
        }

        public ConnectedComponents FindConnectedComponents(string csFileWithClass)
        {
            var result = _classGraphGenerator.CreateGraph(csFileWithClass, false);
            var components = _classGraphAnalyzer.FindConnectedComponents(result);

            return components;
        }

        public ConnectedComponents FindConnectedComponents(SyntaxNode node)
        {
            var result = _classGraphGenerator.CreateGraph(node, false);
            var components = _classGraphAnalyzer.FindConnectedComponents(result);

            return components;
        }
    }
}
