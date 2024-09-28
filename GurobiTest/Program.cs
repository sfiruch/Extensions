using Gurobi;

//https://dominikjeske.github.io/source-generators/

using var env = new GRBEnv();
using var m = new GRBModel(env);
var array = m.AddVars(2, 2, 0, 1, GRB.BINARY);
