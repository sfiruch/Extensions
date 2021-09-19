using Gurobi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
namespace Gurobi
{
    public class GurobiCallback : GRBCallback
    {
        private readonly Action<GurobiCallback> cb;

        public GurobiCallback(Action<GurobiCallback> _cb) => cb = _cb;

        public new void AddLazy(GRBTempConstr _c) => base.AddLazy(_c);
        public new void AddCut(GRBTempConstr _c) => base.AddCut(_c);

#pragma warning disable IDE1006 // Naming Styles
        public new int where
#pragma warning restore IDE1006 // Naming Styles
        {
            get => base.where;
        }

        public new double GetSolution(GRBVar _v) => base.GetSolution(_v);
        public new double UseSolution() => base.UseSolution();
        public new void SetSolution(GRBVar _v, double _val) => base.SetSolution(_v, _val);
        public new string GetStringInfo(int _what) => base.GetStringInfo(_what);
        public new double GetDoubleInfo(int _what) => base.GetDoubleInfo(_what);
        public new double GetIntInfo(int _what) => base.GetIntInfo(_what);
        public new double GetNodeRel(GRBVar _v) => base.GetNodeRel(_v);
        public new void Abort() => base.Abort();

        protected override void Callback() => cb(this);
    }

    public static class GurobiExtensions
    {
        public static IEnumerable<double> AsDouble(this GRBVar[] _var)
        {
            for (var x = 0; x < _var.Length; x++)
                yield return (double)_var[x].Get(GRB.DoubleAttr.X);
        }

        public static GRBLinExpr Sum(this GRBVar[] _vars)
        {
            var res = new GRBLinExpr();
            foreach (var v in _vars)
                res.Add(v);
            return res;
        }

        public static GRBLinExpr Sum(this GRBVar[,] _vars)
        {
            var res = new GRBLinExpr();
            foreach (var v in _vars)
                res.Add(v);
            return res;
        }

        public static GRBLinExpr Sum(this GRBVar[,,] _vars)
        {
            var res = new GRBLinExpr();
            foreach (var v in _vars)
                res.Add(v);
            return res;
        }

        public static GRBLinExpr Sum(this GRBVar[,,,] _vars)
        {
            var res = new GRBLinExpr();
            foreach (var v in _vars)
                res.Add(v);
            return res;
        }

        public static GRBQuadExpr SumL2(this GRBVar[] _vars)
        {
            var res = new GRBQuadExpr();
            foreach (var v in _vars)
                res.Add(v * v);
            return res;
        }

        public static void AddConstr(this GRBModel _m, GRBTempConstr _constr)
        {
            _m.AddConstr(_constr, null);
        }

        public static void AddGenConstrIndicator(this GRBModel _m, GRBVar _binvar, int _binval, GRBTempConstr _constr)
        {
            _m.AddGenConstrIndicator(_binvar, _binval, _constr, null);
        }

        public static void AddGenConstrAnd(this GRBModel _m, GRBVar _resvar, GRBVar[] _vars)
        {
            _m.AddGenConstrAnd(_resvar, _vars, null);
        }

        public static void AddGenConstrOr(this GRBModel _m, GRBVar _resvar, GRBVar[] _vars)
        {
            _m.AddGenConstrOr(_resvar, _vars, null);
        }

        public static void AddGenConstrAbs(this GRBModel _m, GRBVar _resvar, GRBVar _argvar)
        {
            _m.AddGenConstrAbs(_resvar, _argvar, null);
        }

        public static void AddQConstr(this GRBModel _m, GRBTempConstr _constr)
        {
            _m.AddQConstr(_constr, null);
        }

        public static GRBVar AddVar(this GRBModel _m, double lb, double ub, char type, string _prefix = null)
        {
            return _m.AddVar(lb, ub, 0, type, _prefix);
        }

        public static GRBVar[,] AddVars(this GRBModel _m, int _width, int _height, double lb, double ub, char type, string _prefix = null)
        {
            var vars = _m.AddVars(Enumerable.Repeat(lb, _width * _height).ToArray(), Enumerable.Repeat(ub, _width * _height).ToArray(), null, Enumerable.Repeat(type, _width * _height).ToArray(), null);
            _m.Update();

            var i = 0;
            var res = new GRBVar[_width, _height];
            for (var y = 0; y < _height; y++)
                for (var x = 0; x < _width; x++)
                {
                    if (_prefix != null)
                        vars[i].Set(GRB.StringAttr.VarName, $"{_prefix}[{x},{y}]");
                    res[x, y] = vars[i++];
                }
            return res;
        }

        public static GRBVar[,,] AddVars(this GRBModel _m, int _width, int _height, int _depth, double lb, double ub, char type, string _prefix = null)
        {
            var vars = _m.AddVars(Enumerable.Repeat(lb, _width * _height * _depth).ToArray(), Enumerable.Repeat(ub, _width * _height * _depth).ToArray(), null, Enumerable.Repeat(type, _width * _height * _depth).ToArray(), null);
            _m.Update();

            var i = 0;
            var res = new GRBVar[_width, _height, _depth];
            for (var z = 0; z < _depth; z++)
                for (var y = 0; y < _height; y++)
                    for (var x = 0; x < _width; x++)
                    {
                        if (_prefix != null)
                            vars[i].Set(GRB.StringAttr.VarName, $"{_prefix}[{x},{y},{z}]");
                        res[x, y, z] = vars[i++];
                    }
            return res;
        }

        public static GRBVar[,,,] AddVars(this GRBModel _m, int _width, int _height, int _depth, int _d4, double lb, double ub, char type, string _prefix = null)
        {
            var vars = _m.AddVars(Enumerable.Repeat(lb, _width * _height * _depth * _d4).ToArray(), Enumerable.Repeat(ub, _width * _height * _depth * _d4).ToArray(), null, Enumerable.Repeat(type, _width * _height * _depth * _d4).ToArray(), null);
            _m.Update();

            var i = 0;
            var res = new GRBVar[_width, _height, _depth, _d4];
            for (var a = 0; a < _d4; a++)
                for (var z = 0; z < _depth; z++)
                    for (var y = 0; y < _height; y++)
                        for (var x = 0; x < _width; x++)
                        {
                            if (_prefix != null)
                                vars[i].Set(GRB.StringAttr.VarName, $"{_prefix}[{x},{y},{z},{a}]");
                            res[x, y, z, a] = vars[i++];
                        }
            return res;
        }

        public static GRBVar[,,,,] AddVars(this GRBModel _m, int _width, int _height, int _depth, int _d4, int _d5, double lb, double ub, char type, string _prefix = null)
        {
            var vars = _m.AddVars(Enumerable.Repeat(lb, _width * _height * _depth * _d4 * _d5).ToArray(), Enumerable.Repeat(ub, _width * _height * _depth * _d4).ToArray(), null, Enumerable.Repeat(type, _width * _height * _depth * _d4).ToArray(), null);
            _m.Update();

            var i = 0;
            var res = new GRBVar[_width, _height, _depth, _d4, _d5];
            for (var b = 0; b < _d5; b++)
                for (var a = 0; a < _d4; a++)
                    for (var z = 0; z < _depth; z++)
                        for (var y = 0; y < _height; y++)
                            for (var x = 0; x < _width; x++)
                            {
                                if (_prefix != null)
                                    vars[i].Set(GRB.StringAttr.VarName, $"{_prefix}[{x},{y},{z},{a},{b}]");
                                res[x, y, z, a, b] = vars[i++];
                            }
            return res;
        }

        public static GRBVar[] AddVars(this GRBModel _m, int _count, double lb, double ub, char type, string _prefix = null)
        {
            return _m.AddVars(Enumerable.Repeat(lb, _count).ToArray(), Enumerable.Repeat(ub, _count).ToArray(), null, Enumerable.Repeat(type, _count).ToArray(),
                    _prefix == null ? null : Enumerable.Range(0, _count).Select(i => $"{_prefix}[{i}]").ToArray()
                );
        }
    }
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.