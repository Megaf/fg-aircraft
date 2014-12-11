﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace F14AeroPlot
{
    public class ReferenceDocument
    {
        public string Id { get; set; }

        public string Author { get; set; }

        public string Title { get; set; }

        public string Date { get; set; }

        public string Url { get; set; }
    };

    public class BreakPoint
    {
        public BreakPoint(DataElement parent, double iv1, double v)
        {
            Parent = parent;
            data = new Double[1];
            data[0] = iv1;
            this.iv1 = iv1;
            Value = v;
        }
        public BreakPoint(DataElement parent, double iv1, double iv2, double v)
        {
            Parent = parent;
            data = new Double[2];
            data[0] = iv1;
            data[1] = iv2;
            this.iv1 = iv1;
            this.iv2 = iv2;
            Value = v;
        }
        public BreakPoint(DataElement parent, double iv1, double iv2, double iv3, double v)
        {
            Parent = parent;
            data = new Double[3];
            data[0] = iv1;
            data[1] = iv2;
            data[2] = iv3;
            this.iv1 = iv1;
            this.iv2 = iv2;
            this.iv3 = iv3;
            Value = v;
        }

        //    const int MaxDimensions = 3;
        public Double[] data;
        public Double iv1, iv2, iv3;
        public Double Value;
        public string description;

        public DataElement Parent { get; set; }
    }
    public class DataElement
    {
        public DataElement(string iv1, string iv2 = null, string iv3 = null)
        {
            IndependentVars.Add(iv1);
            if (iv2 != null)
                IndependentVars.Add(iv2);
            if (iv3 != null)
                IndependentVars.Add(iv3);
        }

        public List<BreakPoint> data = new List<BreakPoint>();
        public List<String> IndependentVars = new List<string>();
        public void Add(double iv1, double v)
        {
            data.Add(new BreakPoint(this, iv1, v));
        }
        public void Add(double iv1, double iv2, double v)
        {
            data.Add(new BreakPoint(this, iv1, iv2, v));
        }
        public void Add(double iv1, double iv2, double iv3, double v)
        {
            data.Add(new BreakPoint(this, iv1, iv2, iv3, v));
        }
        public object TwoD()
        {
            return data.GroupBy(xx => xx.iv1).Select(xx => new { xx.Key, Val = String.Join(",", xx.Select(yy => yy.Value)) });
        }
        public string description;
        public string Description
        {
            get
            {
                if (!String.IsNullOrEmpty(description)) return description;
                return string.Join(" ", IndependentVars);
            }
        }
        public bool Is2d { get { return IndependentVars.Count == 2; } }

        public string Title { get; set; }
        public List<String> Factors = new List<string>();
        public string Axis;

        internal void AddFactor(string p)
        {
            foreach (var f in p.Split(','))
                Factors.Add(f);
        }

        public string Variable { get; set; }

        public string GetComputeValue(Aerodata aero)
        {
            var vals = new List<String>();
            vals.Add(Variable);
            foreach (var xx in Factors)
            {
                if (aero != null)
                {
                    var name = aero.Lookup(xx);
                    var v = aero.LookupValue(xx);
                    if (v.HasValue)
                    {
                        vals.Add(v.ToString());
                    }
                    else
                    {
                        vals.Add(xx);
                    }
                }
                else
                    vals.Add(xx);
            }
            return string.Join("*", vals);
        }
    }
    public class Aerodata
    {
        public Aerodata()
        {
            Aliases = new Dictionary<string, string>();
            Aliases.Add("P", "velocities/p-aero-rad_sec");
            Aliases.Add("Q", "velocities/q-aero-rad_sec");
            Aliases.Add("R", "velocities/r-aero-rad_sec");

            Aliases.Add("PB", "aero/pb");
            Aliases.Add("QB", "aero/qb");
            Aliases.Add("RB", "aero/rb");

            Aliases.Add("alpha", "aero/alpha-deg");
            Aliases.Add("beta", "aero/beta-deg");
            Aliases.Add("elevator", "fcs/elevator-pos-deg");
            Aliases.Add("rudder", "fcs/rudder-pos-deg");
            Aliases.Add("aileron", "fcs/aileron-pos-deg");
            Aliases.Add("mach", "velocities/mach");
            Aliases.Add("slats", "fcs/slat-pos-norm-deg");
            Aliases.Add("flaps", "fcs/flap-pos-deg");
            Aliases.Add("speedbrake", "fcs/speedbrake-pos-norm");
            Aliases.Add("gear", "gear/gear-pos-norm");

        }
        public Dictionary<string, List<DataElement>> ComputeElems = new Dictionary<string, List<DataElement>>();
        public Dictionary<string, List<string>> ComputeExtra = new Dictionary<string, List<string>>();
        public void Compute(string axis, DataElement[] dataElement)
        {
            if (!ComputeElems.ContainsKey(axis))
                ComputeElems[axis] = new List<DataElement>();
            foreach (var el in dataElement)
            {
                ComputeElems[axis].Add(el);
                el.Axis = axis;
            }
            // Add list of elements to axis
        }

        public void Compute(string axis, string p2)
        {
            foreach (var el in p2.Split(','))
            {
                ComputeExtra[axis] = new List<string>();
                ComputeExtra[axis].Add(el);
            }
        }

        public void AddVariable(string p1, string p2)
        {
            Variables[p1] = p2;
        }

        public void AddConstant(string p1, double val)
        {
            Constants[p1] = val;
        }

        public string GetCompute(string axis)
        {
            var s1 = axis + "=" + String.Join("+", ComputeElems[axis].Select(xx => xx.GetComputeValue(this)));
            string s2 = "";
            if (ComputeExtra.ContainsKey(axis))
                s2 = String.Join("+", ComputeExtra[axis]);
            if (!String.IsNullOrEmpty(s2))
                return s1 + " + " + s2;
            return s1;

        }

        public Dictionary<string, string> GetCompute()
        {
            var rv = new Dictionary<string, string>();
            foreach (var axis in ComputeElems)
            {
                rv[axis.Key] = GetCompute(axis.Key);
            }
            return rv;
        }

        public Dictionary<String, double> Constants = new Dictionary<string, double>();
        public Dictionary<String, String> Variables = new Dictionary<string, String>();
        public List<ReferenceDocument> References = new List<ReferenceDocument>();
        public string Title { get; set; }
        public string Lookup(string coeff)
        {
            if (Constants.ContainsKey(coeff))
                return Constants[coeff].ToString() + String.Format(" <!-- {0} -->", coeff);
            if (Variables.ContainsKey(coeff))
                coeff = Variables[coeff];
            if (Aliases.ContainsKey(coeff))
                coeff = Aliases[coeff];
            return coeff;
        }


        public bool IsUsed(DataElement aero_element)
        {
            return aero_element.data.Any();
        }

        public bool Is2d(DataElement aero_element)
        {
            return aero_element.IndependentVars.Count == 2;
        }

        public bool Is3d(DataElement aero_element)
        {
            return aero_element.IndependentVars.Count == 3;
        }

        public Dictionary<string, string> Aliases { get; set; }
        public Dictionary<string, DataElement> Data = new Dictionary<string, DataElement>();

        public DataElement Add(string title, string element, string iv1, string iv2 = null, string iv3 = null)
        {
            var ne = new DataElement(iv1, iv2, iv3)
            {
                Title = title,
                Variable = element,
            };
            Data[element] = ne;
            return ne;
        }
        public void AddDataPoint(string element, double iv1, double iv2, double iv3, double v)
        {
            Data[element].Add(iv1, iv2, iv3, v);
        }

        /// <summary>
        /// If the value looked up is a numeric constant then convert and return as a double
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>

        internal double? LookupValue(string name)
        {
            double v;
            if (Char.IsDigit(name.Trim()[0]))
            {
                if (Double.TryParse(name.TruncateAt(" "), out v))
                    return v;
                else
                    return null;
            }
            return null;
        }
    }

    public class F15Aero : Aerodata
    {

        public static F15Aero Create()
        {
            var aerodata = new F15Aero();
            aerodata.Aliases.Add("DELEDD", "fcs/elevator-pos-deg");
            aerodata.Aliases.Add("DTALD", "fcs/differential-elevator-pos-deg"); // -ve is left.
            aerodata.Aliases.Add("DDA", "fcs/aileron-pos-deg");
            aerodata.Aliases.Add("DRUDD", "fcs/rudder-pos-deg");
            aerodata.Aliases.Add("BETA", "aero/beta-deg");

            aerodata.References.Add(new ReferenceDocument
            {
                Id = "RJH-ZARETTO-2014-12-F-15-Aero",
                Author = "Richard Harrison",
                Date = "December, 2014",
                Title = "F-15 Aerodynamic data from  (AFIT/GAE/ENY/90D-16); CG 25.65%",
                Url = "http://www.zaretto.com/sites/zaretto.com/files/F-15-data/rjh-zaretto-f-15-aerodynamic-data.pdf",

            });
            aerodata.References.Add(new ReferenceDocument
            {
                Id = "AFIT/GAE/ENY/90D-16",
                Author = "Robert J. McDonnell, B.S., Captain, USAF",
                Date = "December 1990",
                Title = "INVESTIGATION OF THE HIGH ANGLE OF ATTACK DYNAMICS OF THE F-15B USING BIFURCATION ANALYSIS",
                Url = "http://www.zaretto.com/sites/zaretto.com/files/F-15-data/ADA230462.pdf",
            });
            aerodata.Title = "F-15 Aerodynamic data from  (AFIT/GAE/ENY/90D-16); CG 25.65%";
            var EPA43 = aerodata.Add("CNDR MULTIPLIER CLDR, CYDR DUE TO SPEEDBRAKE", "EPA43", "alpha", "speedbrake");
            //var EPA02S = aerodata.Add("BETA MULTIPLIER TABLE (S)", "EPA02S", "beta");
            //var EPA02L = aerodata.Add("BETA MULTIPLIER TABLE (L)", "EPA02L", "beta");

            var CFZ = aerodata.Add("BASIC LIFT", "CFZB", "alpha", "elevator");
//            var CFZDE = aerodata.Add("LIFT INCREMENT DUE TO ELEVATOR DEFLECTION", "CFZDE", "elevator");
            var CFX = aerodata.Add("BASIC DRAG", "CFXB", "alpha", "elevator");
//            var CFXDE = aerodata.Add("DRAG INCREMENT DUE TO ELEVATOR DEFLECTION", "CFXDE", "elevator");
            //System.Console.WriteLine("CFDE ");
            //for (var DELE = -40; DELE <= 40; DELE += 5)
            //{
            //    var DELESR = DELE / DTOR;
            //    CFZDE.Add(DELE, (0.40781955 * DELESR) + (0.10114579 * (DELESR * DELESR)));
            //    CFXDE.Add(DELE, (0.20978902 * DELESR) + (0.30604211 * (DELESR * DELESR)) + 0.09833617);
            //}

            var CFYB = aerodata.Add("BASIC SIDE FORCE", "CFYB", "alpha", "beta", "elevator");
            //            CFYB.AddFactor("EPA02L");

            var CFYP = aerodata.Add("SIDE FORCE DUE TO ROLL RATE (CYP)", "CFYP", "alpha");
            CFYP.AddFactor("PB");

            var CFYR = aerodata.Add("SIDE FORCE DUE TO YAW RATE (CYR)", "CFYR", "alpha");
            CFYR.AddFactor("RB");

            var CYDAD = aerodata.Add("SIDE FORCE DUE TO AILERON DEFLECTION", "CYDAD", "alpha");
            CYDAD.AddFactor("DDA");

            var CYDRD = aerodata.Add("SIDE FORCE DUE TO RUDDER DEFLECTION", "CYDRD", "alpha", "rudder");
            CYDRD.AddFactor("DRUDD,DRFLX5,EPA43");

            var CYDTD = aerodata.Add("SIDE FORCE DUE TO DIFFERETIAL TAIL DEFLECTION - CYDTD", "CYDTD", "alpha", "elevator");
            CYDTD.AddFactor("DTFLX5,0.3,DTALD");

            var CYRB = aerodata.Add("ASYMMETRIC CY AT HIGH ALPHA", "CYRB", "alpha", "beta");
            // CLM
            var CML1 = aerodata.Add("BASIC ROLLING MOMENT - CL(BETA)", "CML1", "alpha", "beta");
            //            CML1.AddFactor("EPA02S");

            var CMLP = aerodata.Add("ROLL DAMPING DERIVATIVE - CLP", "CMLP", "alpha");
            CMLP.AddFactor("PB");

            var CMLR = aerodata.Add("ROLLING MOMENT DUE TO YAW RATE - CLR", "CMLR", "alpha");
            CMLR.AddFactor("RB");

            var CLDAD = aerodata.Add("ROLLING MOMENT DUE TO AILERON DEFLECTION (CLDA)", "CMLDAD", "alpha");
            CLDAD.AddFactor("DDA");

            var CLDRD = aerodata.Add("ROLLING MOMENT DUE TO RUDDER DEFLECTION -(CLD)", "CMLDRD", "alpha", "rudder");
            CLDRD.AddFactor("DRUDD,DRFLX1,EPA43");

            var CLDTD = aerodata.Add("ROLLING MOMENT DUE TO DIFFERENTIAL TAIL DEFLECTION - CLDD", "CMLDTD", "alpha", "elevator");
            CLDTD.AddFactor("DTFLX1,0.3,DTALD");

            var DCLB = aerodata.Add("DELTA CLB DUE TO 2-PLACE CANOPY", "CMLDCLB", "alpha");
            DCLB.AddFactor("BETA");

            // CMM
            var CMM1 = aerodata.Add("BASIC PITCHING MOMENT - CM", "CMM1", "alpha", "elevator");

            var CMMQ = aerodata.Add("PITCH DAMPING DERIVATIVE - CMQ", "CMMQ", "alpha");
            CMMQ.AddFactor("QB");
            //
            var CMN1 = aerodata.Add("BASIC YAWING MOMENT - CN (BETA)", "CMN1", "alpha", "beta", "elevator");
            //CMN1.AddFactor("EPA02S");

            var CNDRDr = aerodata.Add("YAWING MOMENT DUE TO RUDDER DEFLECTION -CNDR", "CMNDRDr", "alpha", "beta");
            CNDRDr.AddFactor("DRUDD,DRFLX3,EPA43");

            //var CNDRDe = aerodata.Add("YAWING MOMENT DUE TO RUDDER DEFLECTION ELEVATOR INCREMENT -CNDRe", "CMNDRDe", "alpha", "beta", "elevator");
            //CNDRDe.AddFactor("DELEDD,DRFLX3,EPA43");

            var CMNP = aerodata.Add("YAWING MOMENT DUE TO ROLL RATE - CNP", "CMNP", "alpha");
            CMNP.AddFactor("PB");

            var CMNR = aerodata.Add("YAW DAMPING DERIVATIVE - CMNR", "CMNR", "alpha");
            CMNR.AddFactor("RB");

            var CNDTD = aerodata.Add("YAWING MOMENT DUE TO DIFFERENTIAL TAIL DEFLECTION - CNDDT", "CMNDTD", "alpha", "elevator");
            CNDTD.AddFactor("DTFLX3,DTALD");
            //            var CNDAD = aerodata.Add("", "CNDAD", "alpha", "aileron");
            var CNDAD = aerodata.Add("YAWING MOMENT DUE TO AILERON DEFLECTION -CNDA", "CMNDAD", "alpha");
            CNDAD.AddFactor("DDA");

            var CNRB = aerodata.Add("ASYMMETRIC CN AT HIGH ALPHA", "CMNRB", "alpha", "beta");

            aerodata.AddConstant("DCNB", -2.5e-4);
            aerodata.AddConstant("DTFLX1", 0.975);
            aerodata.AddConstant("DRFLX1", 0.85);
            aerodata.AddConstant("DTFLX3", 0.975);
            aerodata.AddConstant("DRFLX3", 0.89);
            aerodata.AddConstant("DTFLX5", 0.975);
            aerodata.AddConstant("DRFLX5", 0.89);
            //aerodata.AddVariable("DELEDD", "0.3*DDA");

            aerodata.Compute("LIFT", new[] { CFZ, /* CFZDE */ });
            aerodata.Compute("DRAG", new[] { CFX, /* CFXDE */ });
            aerodata.Compute("SIDE", new[] { CFYB, CYDAD, CYDRD, CYDTD, CFYP, CFYR });
            aerodata.Compute("ROLL", new[] { CML1, CLDAD, CLDRD, CLDTD, CMLP, CMLR, DCLB });
            aerodata.Compute("ROLL", "(DLNB*BETA)");
            aerodata.Compute("PITCH", new[] { CMM1, CMMQ });
            aerodata.Compute("YAW", new[] { CMN1, CNDAD, /*CNDRDe,*/ CNDRDr, CNDTD, CMNP, CMNR });
            aerodata.Compute("YAW", "(DCNB*BETA)");

            var DTOR = 180.0 / Math.PI;
            var DEGRAD = DTOR;


            //var aoa = new [] {0,5,10,15,20,25,30,35,40,45,50,55,60};
            for (var alpha = -15; alpha <= 60; alpha = inc(alpha))
            {
                var RAL = alpha / DTOR;

                for (var beta1 = -20; beta1 <= 20; beta1 = incbeta(beta1))
                {

                    //var RARUD=0.0;
                    var RBETA = beta1 / DTOR;
                    var RABET = Math.Abs(beta1) / DTOR;
                    for (double DELESD = -30.0; DELESD <= 30; DELESD += 5)
                    {
                        var DELESR = DELESD / DTOR;
                        //var DTFLX5 = 0.975;
                        //var DRFLX5 = 0.89; 
                        CFYB.Add(alpha, beta1, DELESD, GetEPA02L(beta1) * (-0.05060386 - (0.12342073 * RAL) + (1.04501136 * RAL * RAL)
                        - (0.17239516 * Math.Pow(RAL, 3)) - (2.90979277 * Math.Pow(RAL, 4))
                        + (3.06782935 * Math.Pow(RAL, 5)) - (0.58422116 * Math.Pow(RAL, 6))
                        - (0.06578812 * RAL * RABET) - (0.71521988 * RABET) - (0.00000475273
                        * (RABET * RABET)) - (0.04856168 * RAL * DELESR) - (0.05943607 * RABET * DELESR) +
                        (0.02018534 * DELESR)));

                        CMN1.Add(alpha, beta1, DELESD, GetEPA02S(beta1) * (0.01441512 + (0.02242944 * RAL) - (0.30472558 * Math.Pow(RAL, 2))
                        + (0.14475549 * Math.Pow(RAL, 3))
                        + (0.93140112 * Math.Pow(RAL, 4)) - (1.52168677 * Math.Pow(RAL, 5)) +
                        (0.90743413 * Math.Pow(RAL, 6)) - (0.16510989 * Math.Pow(RAL, 7))
                        - (0.0461968 * Math.Pow(RAL, 8))
                        + (0.01754292 * Math.Pow(RAL, 9)) - (0.17553807 * RAL * RABET) +
                        (0.15415649 * RAL * RABET * DELESR)
                        + (0.14829547 * Math.Pow(RAL, 2) * Math.Pow(RABET, 2))
                        - (0.11605031 * Math.Pow(RAL, 2) * RABET * DELESR)
                        - (0.06290678 * Math.Pow(RAL, 2) * Math.Pow(DELESR, 2))
                        - (0.01404857 * Math.Pow(RAL, 2) * Math.Pow(DELESR, 2))
                        + (0.07225609 * RABET) - (0.08567087 * Math.Pow(RABET, 2))
                        + (0.01184674 * Math.Pow(RABET, 3))
                        - (0.00519152 * RAL * DELESR) + (0.03865177 * RABET * DELESR)
                        + (0.00062918 * DELESR)));
                    }
                    {
                        var DELESR = 1;
                        CNDRDr.Add(alpha, beta1, -0.00153402 + (0.00184982 * RAL) - (0.0068693 * RAL * RAL)
                        + (0.01772037 * Math.Pow(RAL, 3))
                        + (0.03263787 * Math.Pow(RAL, 4)) - (0.15157163 * Math.Pow(RAL, 5)) + (0.18562888
                        * Math.Pow(RAL, 6)) - (0.0966163 * Math.Pow(RAL, 7)) + (0.0185916 * Math.Pow(RAL, 8)) + (0.0002587
                        * RAL * DELESR) - (0.00018546 * RAL * DELESR * RBETA) - (0.00000517304 * RBETA)
                        - (0.001202718 * RAL * RBETA) - (0.0000689379 * RBETA * DELESR)
                            // -(0.00040536*RBETA*RARUD)-(0.00000480484*DELESR*RARUD)
                            //-(0.00041786*RAL*RARUD)
                            //+(0.0000461872*RBETA)+(0.00434094*Math.Pow(RBETA,2))
                            //-(0.00490777*Math.Pow(RBETA,3))
                            //+(0.000005157867*RARUD)+(0.00225169*RARUD*RARUD)-(0.00208072*Math.Pow(RARUD,3))
                        );
                    }
                    //
                    {
                        var RALN1 = 0.69813;
                        var RALN2 = 90 / DTOR;
                        var RBETN1 = -0.174532;
                        var RBETN2 = 0.34906;

                        var AN = 0.034;
                        var ASTARN = 1.0472;
                        var BSTARN = 0.087266;
                        var ZETAN = (2.0 * ASTARN - (RALN1 + RALN2)) / (RALN2 - RALN1);
                        var ETAN = (2 * BSTARN - (RBETN1 + RBETN2)) / (RBETN2 - RBETN1);
                        var XN = (2.0 * RAL - (RALN1 + RALN2)) / (RALN2 - RALN1);
                        var YN = (2.0 * RBETA - (RBETN1 + RBETN2)) / (RBETN2 - RBETN1);
                        var FN = ((5 * Math.Pow(ZETAN, 2)) - (4 * ZETAN * XN) - 1) *
                        Math.Pow((XN * XN) - 1, 2) / Math.Pow(((ZETAN * ZETAN) - 1), 3);
                        var GN = ((5 * (ETAN * ETAN)) - (4 * ETAN * YN) - 1.0) *
                        Math.Pow(((YN * YN) - 1), 2) / Math.Pow((ETAN * ETAN) - 1, 3);
                        if (RAL < 0.69813)
                            CNRB.Add(alpha, beta1, 0);
                        else if (RBETA < 0.174532 || RBETA > 0.34096)
                            CNRB.Add(alpha, beta1, 0);
                        else
                            CNRB.Add(alpha, beta1, AN * FN * GN);
                    }
                }
                // END of beta IV terms

                // Terms (alpha,elevator)
                for (double DELESD = -30.0; DELESD <= 30; DELESD += 5)
                {
                    var DELESR = DELESD / DTOR;

                var CFZ1 = -0.00369376 + (3.78028702 * RAL) + (0.6921459 * RAL * RAL)
                            - (5.0005867 * (Math.Pow(RAL, 3))) + (1.94478199 * (Math.Pow(RAL, 4))
                            + (0.40781955 * DELESR) + (0.10114579 * (DELESR * DELESR))
                            );

                CFZ.Add(alpha, DELESD, CFZ1);
                var CL = CFZ1 / 57.29578;
                var CFX1 = 0.01806821 + (0.01556573 * CL) + (498.96208868 * CL * CL)
                - (14451.56518396 * (Math.Pow(CL, 3))) + (2132344.6184755 * (Math.Pow(CL, 4)));
                // TRANSITIONING FROM  LOW AOA DRAG TABLE TO HIGH AOA DRAG TABLE
                var CFX2 = 0.0267297 - (0.10646919 * RAL) + (5.39836337 * RAL * RAL) -
                            (5.0086893 * Math.Pow(RAL, 3)) + (1.34148193 * Math.Pow(RAL, 4)
                            + (0.20978902 * DELESR) + (0.30604211 * (DELESR * DELESR)) + 0.09833617);
                {
                    var A1 = 20.0 / DEGRAD;
                    var A2 = 30.0 / DEGRAD;
                    var A12 = A1 + A2;
                    var BA = 2.0 / (-Math.Pow(A1, 3) + 3.0 * A1 * A2 * (A1 - A2) + Math.Pow(A2, 3));
                    var BB = -3.0 * BA * (A1 + A2) / 2.0;
                    var BC = 3 * BA * A1 * A2;
                    var BD = BA * A2 * A2 * (A2 - 3.0 * A1) / 2.0;
                    var F1 = BA * Math.Pow(RAL, 3) + BB * RAL * RAL + BC * RAL + BD;
                    var F2 = -BA * Math.Pow(RAL, 3) + (3.0 * A12 * BA + BB) * Math.Pow(RAL, 2) - (BC + 2 * A12 * BB + 3 * Math.Pow(A12, 2) * BA) * RAL + BD + A12 * BC + A12 * A12 * BB + Math.Pow(A12, 3) * BA;
                    //var F2=-BA*(A2*A2)*(A2-3*A12*BA+BB)*(RAL*RAL)-(BC+2*A12*BB+3*A12*A12*BA)*RAL+
                    //BD+A12*BC+A12*A12*BB+Math.Pow(A12,3)*BA;
                    if (RAL < A1) CFX.Add(alpha, DELESD, CFX1);
                    else if (RAL > A2) CFX.Add(alpha, DELESD, CFX2);
                    else CFX.Add(alpha, DELESD, CFX1 * F1 + CFX2 * F2);
                }


                    CMM1.Add(alpha, DELESD, 0.00501496 - (0.080491 * RAL) - (1.03486675 * RAL * RAL)
                                   - (0.68580677 * Math.Pow(RAL, 3)) + (6.46858488 * Math.Pow(RAL, 4))
                                   - (10.15574108 * Math.Pow(RAL, 5)) +
                                   (6.44350808 * Math.Pow(RAL, 6)) - (1.46175188 * Math.Pow(RAL, 7))
                                   + (0.24050902 * RAL * DELESR)
                                   - (0.42629958 * DELESR) - (0.03337449 * DELESR)
                                   - (0.53951733 * Math.Pow(DELESR, 3)));

                    CNDTD.Add(alpha, DELESD, 0.00058286 + (0.0007341 * RAL) - (0.00746113 * RAL * RAL)
                    - (0.00685223 * Math.Pow(RAL, 3))
                    + (0.03277271 * Math.Pow(RAL, 4)) - (0.02791456 * Math.Pow(RAL, 5))
                    + (0.00732915 * Math.Pow(RAL, 6))
                    + (0.00120456 * RAL * DELESR) - (0.00168102 * DELESR) + (0.0006462 *
                    +DELESR * DELESR));
                }
                //for (var beta1 = -20; beta1 <= 20; beta1 += 5)
                //{
                //    var DELESR = 1;
                //    var RBETA = beta1 / DTOR;
                //    for (var rudder = -30; rudder <= 30; rudder += 5)
                //    {
                //        var RARUD = Math.Abs(rudder) / DTOR;
                //        CNDRDr.Add(alpha, beta1, rudder,
                //        -(0.00040536 * RBETA * RARUD) - (0.00000480484 * DELESR * RARUD)
                //        - (0.00041786 * RAL * RARUD)
                //        + (0.0000461872 * RBETA) + (0.00434094 * Math.Pow(RBETA, 2))
                //        - (0.00490777 * Math.Pow(RBETA, 3))
                //        + (0.000005157867 * RARUD) + (0.00225169 * RARUD * RARUD) - (0.00208072 * Math.Pow(RARUD, 3))
                //        );
                //    }
                //}

                if (RAL < 0.52359998 && alpha >= -20)
                {
                    CFYP.Add(alpha, 0.014606188 + (2.52405055 * RAL) - (5.02687473 * Math.Pow(RAL, 2))
                    - (106.43222962 * Math.Pow(RAL, 3)) + (256.80215423 * Math.Pow(RAL, 4))
                    + (1256.39636248 * Math.Pow(RAL, 5))
                    - (3887.92878173 * Math.Pow(RAL, 6)) - (2863.16083460 * Math.Pow(RAL, 7)) +
                    (17382.72226362 * Math.Pow(RAL, 8)) - (13731.65408408 * Math.Pow(RAL, 9)));
                }
                else if ((RAL >= 0.52359998) && (RAL <= 0.610865))
                {
                    CFYP.Add(alpha, 0.00236511 + (0.52044678 * (RAL - 0.52359998)) - (12.8597002 * Math.Pow(RAL - 0.52359998, 2)) + (75.46138 * Math.Pow(RAL - 0.52359998, 3)));
                }
                else if (RAL > 0.610865)
                    CFYP.Add(alpha, 0);

                if (alpha > 30)
                    CFYR.Add(alpha, 0);
                else if (RAL < -0.06981)
                    CFYR.Add(alpha, 0.35);
                else if (RAL >= -0.06981 && RAL < 0)
                    CFYR.Add(alpha, 0.34999999 + (35.4012413 * Math.Pow(RAL + 0.06981, 2)) - (493.33441162 * +Math.Pow(RAL + 0.06981, 3)));
                else if (RAL >= 0 && RAL <= 0.523599)
                    CFYR.Add(alpha, 0.35468605 - (2.26998141 * RAL) + (51.82178387 * RAL * RAL)
                    - (718.55069823 * Math.Pow(RAL, 3))
                    + (4570.004921721 * Math.Pow(RAL, 4)) - (14471.58028351 * Math.Pow(RAL, 5)) +
                    (22026.58930662 * Math.Pow(RAL, 6)) - (12795.99029404 * Math.Pow(RAL, 7)));
                else if (RAL > 0.523599 && RAL < 0.61087)
                    CFYR.Add(alpha, 0.00193787 + (1.78332495 * (RAL - 0.52359903)) - (41.63198853 * Math.Pow(RAL - 0.52359903, 2)) + (239.97909546 * Math.Pow(RAL, 3)));
                ///
                if (RAL < 0.55851)
                    CYDAD.Add(alpha, -0.00020812 + (0.00062122 * RAL) + (0.00260729 * RAL * RAL)
                    + (0.00745739 * Math.Pow(RAL, 3)) - (0.0365611 * Math.Pow(RAL, 4))
                    - (0.04532683 * Math.Pow(RAL, 5)) + (0.20674845 * Math.Pow(RAL, 6))
                    - (0.13264434 * Math.Pow(RAL, 7)) - (0.00193383 * Math.Pow(RAL, 8)));
                else if (RAL >= 0.55851 && RAL < 0.61087)
                    CYDAD.Add(alpha, 0.00023894 + (0.00195121 * (RAL - 0.55851001))
                    * (0.02459273 * Math.Pow(RAL - 0.55851001, 2)) - (0.1202244 * Math.Pow(RAL - 0.55851001, 3)));
                else if (RAL >= 0.61087)
                    CYDAD.Add(alpha, 0.27681285 - (2.02305395 * RAL) + (6.01180715 * RAL * RAL)
                    - (9.24292188 * Math.Pow(RAL, 3)) + (7.59857819 * Math.Pow(RAL, 4))
                    - (2.8565527 * Math.Pow(RAL, 5)) + (0.25460503 * Math.Pow(RAL, 7))
                    - (0.01819815 * Math.Pow(RAL, 9)));

                //
                //double EPA43;
                // NOTE  - THE  PARAMETER  EPA43  IS  A MULTIPLIER  ON  RUDDER
                // EFFECTIVENESS  DUE TO SPEEBRAKE.  THIS  TABLE  IS  ALSO
                // LIMITED  TO  36  Deg  AOA.  HOWEVER, THERE IS NO AERODYNAMIC  EFECT
                // FOR  ANGLES  OF  ATTACK  LESS  THAN  16  DEG
                // AND  SPEEDERAKE IS AUTOMATICALLY  RETIRACTED  AT  AOA
                // GREATER  THAN  15  DEG.  THEREFORE  THIS TABLE  SHOULD NOT BE NECESARY 
                // FOR NORMAL OPERATION 
                if (alpha >= 0 && alpha < 45)
                {
                    for (var DSPBD = 0; DSPBD <= 70; DSPBD += 5)
                    {
                        var DSPBR = DSPBD / DTOR;
                        if (RAL <= 0.0)
                            EPA43.Add(alpha, DSPBD, 1);
                        else if (RAL > 15 / DTOR)
                        {
                            if (DSPBD == 0)
                                EPA43.Add(alpha, DSPBD, 1);
                            // changed this to have 15 as original data didn't.
                            else if (RAL <= 0.6283185)  //0.6283185  RADIANS  = 36  DEG
                                EPA43.Add(alpha, DSPBD, 0.9584809 + (4.13369452 * RAL) - (18.31288396 * RAL * RAL) + (19.5511466 * Math.Pow(RAL, 3)) - (1.09295946 * RAL * DSPBR) + (0.17441033 * DSPBR * DSPBR));
                            else if (RAL > 0.6283185)
                                EPA43.Add(alpha, DSPBD, 1);
                        }
                    }

                    if (RAL < 0.55851)
                        CMNP.Add(alpha, -0.00635409 - (1.14153932 * RAL) + (2.82119027 * Math.Pow(RAL, 2)) +
                        (54.4739579 * Math.Pow(RAL, 3)) - (140.89527667 * Math.Pow(RAL, 4)) - (676.73746128 *
                        Math.Pow(RAL, 5)) + (2059.18263976 * Math.Pow(RAL, 6)) + (1579.41664748 * Math.Pow(RAL, 7))
                        - (8933.08535712 * Math.Pow(RAL, 8)) + (6806.54761267 * Math.Pow(RAL, 9)));
                    else if (RAL >= 0.55851 && RAL <= 0.61087)
                        CMNP.Add(alpha, -0.07023239 + (1.085815 * (RAL - 0.55851))
                        + (8.8526521 * Math.Pow(RAL - 0.55851, 2)) - (192.6093 * Math.Pow(RAL - 0.55851, 3)));
                    else if (RAL > 0.61087)
                        CMNP.Add(alpha, -71.03693533 + (491.32506715 * RAL)
                        - (1388.11177979 * Math.Pow(RAL, 2)) +
                        (2033.48621905 * Math.Pow(RAL, 3))
                        - (1590.91322362 * Math.Pow(RAL, 4)) + (567.38432316 * Math.Pow(RAL, 5))
                        - (44.97702536 * Math.Pow(RAL, 7)) + (2.8140669 * Math.Pow(RAL, 9)));

                    if (RAL <= -0.069813)
                        CMNR.Add(alpha, -0.28050);
                    else if (RAL > -0.069813 && RAL < 0.0)
                        CMNR.Add(alpha, -0.2804999948 + (35.9903717041 * Math.Pow(RAL + 0.0698129982, 2))
                        - (516.1574707031 * Math.Pow(RAL + 0.0698129982, 3)));
                    else if (RAL >= 0.0 && RAL <= 0.78539801)
                        CMNR.Add(alpha, -0.28071511 - (2.52183924 * RAL) + (68.90860031 * Math.Pow(RAL, 2))
                        - (573.23100511 * Math.Pow(RAL, 3)) + (2009.08725005 * Math.Pow(RAL, 4))
                        - (3385.15675307 * Math.Pow(RAL, 5))
                        + (2730.49473149 * Math.Pow(RAL, 6)) - (848.12322034 * Math.Pow(RAL, 7)));
                    else if (RAL > 0.78539801 && RAL < 0.95993102)
                        CMNR.Add(alpha, -0.1096954 + (0.52893072 * (RAL - 0.78539801)) - (6.09109497 * Math.Pow(RAL -
                        0.78539801, 2)) + (17.47834015 * Math.Pow(RAL - 0.78539801, 3)));
                    else
                        CMNR.Add(alpha, -0.11);


                }

                for (var rudder = -30; rudder <= 30; rudder += 5)
                {
                    var RARUD = rudder / DTOR;
                    CYDRD.Add(alpha, rudder, 0.00310199 + (0.00119963 * RAL) + (0.02806933 * RAL * RAL)
                    - (0.12408447 * Math.Pow(RAL, 3)) - (0.12032121 * Math.Pow(RAL, 4))
                    + (0.79150279 * Math.Pow(RAL, 5)) - (0.86544347 * Math.Pow(RAL, 6))
                    + (0.27845115 * Math.Pow(RAL, 7)) + (0.00122999 * RAL * RARUD) + (0.00145943
                    * RARUD) - (0.01211427 * RARUD * RARUD) + (0.00977937 * Math.Pow(RARUD, 3)));

                    CLDRD.Add(alpha, rudder, 0.00013713 - (0.000035439 * RAL) - (0.00227912 * RAL * RAL)
                    + (0.00742636 * Math.Pow(RAL, 3)) + (0.00991839 * Math.Pow(RAL, 4))
                    - (0.04711846 * Math.Pow(RAL, 5)) + (0.046124 * Math.Pow(RAL, 6))
                    - (0.01379021 * Math.Pow(RAL, 7)) + (0.0003678685 * RARUD * RAL) +
                    +(0.00001043751 * RARUD) - (0.00015866 * RARUD * RARUD) + (0.00016133
                    * Math.Pow(RARUD, 3)));

                }
                for (double DELESD = -30.0; DELESD <= 30; DELESD += 5)
                {
                    var DELESR = DELESD / DTOR;
                    CYDTD.Add(alpha, DELESD, -0.00157745 - (0.0020881 * RAL) + (0.00557239 * RAL * RAL)
                    - (0.00139886 * Math.Pow(RAL, 3)) + (0.04956247 * Math.Pow(RAL, 4))
                    - (0.0135353 * Math.Pow(RAL, 5)) - (0.11552397 * Math.Pow(RAL, 6))
                    + (0.1443452 * Math.Pow(RAL, 7)) - (0.05072189 * Math.Pow(RAL, 8)) - (0.01061113 *
                    Math.Pow(RAL, 3) * Math.Pow(DELESR, 3) - (0.00010529 * RAL * RAL * DELESR * DELESR))
                    - (0.00572463 * RAL * DELESR * DELESR)
                    + (0.01885361 * RAL * RAL * DELESR) - (0.01412258 * RAL * Math.Pow(DELESR, 2))
                    - (0.00081776 * DELESR) + (0.00404354 * Math.Pow(DELESR, 2)) -
                    (0.0212189 * Math.Pow(DELESR, 3)) + (0.00655063 * Math.Pow(DELESR, 4))
                    + (0.03341584 * Math.Pow(DELESR, 5)));

                    //CLDAD.Add(alpha, DELESD, 0.005762 + (0.0003847 * RAL) - (0.00502091 * RAL * RAL)
                    //+ (0.00161407 * Math.Pow(RAL, 3)) + (0.02268829 * Math.Pow(RAL, 4))
                    //- (0.03935269 * Math.Pow(RAL, 5)) + (0.02472827 * Math.Pow(RAL, 6))
                    //- (0.00543345 * Math.Pow(RAL, 7)) + (0.0000007520348 * DELESR * RAL) +
                    //(0.000000390773 * DELESR));
                    CLDTD.Add(alpha, DELESD, 0.00066663 + (0.00074174 * RAL) + (0.00285735 * RAL * RAL)
                    - (0.02030692 * Math.Pow(RAL, 3)) - (0.00352997 * Math.Pow(RAL, 4))
                    + (0.0997962 * Math.Pow(RAL, 5)) - (0.14591227 *
                    Math.Pow(RAL, 6)) + (0.08282004 * Math.Pow(RAL, 7))
                    - (0.0168667 * Math.Pow(RAL, 8)) + (0.00306142 * Math.Pow(RAL, 3) * DELESR)
                    - (0.00110266 * RAL * RAL * Math.Pow(DELESR, 2)) + (0.00088031 * RAL *
                    Math.Pow(DELESR, 2)) - (0.00432594 * RAL * RAL * DELESR) -
                    (0.00720141 * RAL * Math.Pow(DELESR, 3))
                    - (0.00034325 * DELESR) + (0.00033433 * Math.Pow(DELESR, 2)) + (0.00800183
                    * Math.Pow(DELESR, 3)) - (0.00555986 * Math.Pow(DELESR, 4)) - (0.01841172 * Math.Pow(DELESR, 5)));

                }
                CLDAD.Add(alpha, 0.005762 + (0.0003847 * RAL) - (0.00502091 * RAL * RAL)
                + (0.00161407 * Math.Pow(RAL, 3)) + (0.02268829 * Math.Pow(RAL, 4))
                - (0.03935269 * Math.Pow(RAL, 5)) + (0.02472827 * Math.Pow(RAL, 6))
                - (0.00543345 * Math.Pow(RAL, 7))
                    //                + (0.0000007520348 * DELESR * RAL) + (0.000000390773 * DELESR)
                );

                for (var beta1 = -20; beta1 <= 20; beta1 += 5)
                {
                    var RABET = Math.Abs(beta1) / DTOR;
                    //CYRB (alpha,beta)
                    var RBETA = RABET;
                    var RALY1 = 0.6108652;
                    var RALY2 = 90 / DEGRAD;
                    var RBETY1 = -0.0872565;
                    var RBETY2 = 0.1745329;

                    var AY = 0.1640;
                    var ASTARY = 0.95993;
                    var BSTARY = 0.087266;
                    var ZETAY = (2.0 * ASTARY - (RALY1 + RALY2)) / (RALY2 - RALY1);
                    var ETAY = (2.0 * BSTARY - (RBETY1 + RBETY2)) / (RBETY2 - RBETY1);

                    var X = (2.0 * RAL - (RALY1 + RALY2)) / (RALY2 - RALY1);
                    var Y = (2.0 * RBETA - (RBETY1 + RBETY2)) / (RBETY2 - RBETY1);
                    var FY = ((5 * Math.Pow(ZETAY, 2)) - (4 * ZETAY * X) - 1.0) * Math.Pow(((X * X) - 1)
                    , 2) * (1.0 / Math.Pow(((ZETAY * ZETAY) - 1), 3));
                    var GY = ((5.0 * (ETAY * ETAY)) - (4.0 * ETAY * Y) - 1) * Math.Pow(((Y * Y) - 1), 2)
                    * (1.0 / Math.Pow(((ETAY * ETAY) - 1), 3));
                    var cyrb = AY * FY * GY;
                    if (RAL < 0.6108652)
                        cyrb = 0;
                    if (RAL < -0.0872665 || RBETA > 0.1745329)
                        cyrb = 0;
                    CYRB.Add(alpha, beta1, cyrb);
                    //var CFY=(CFY1*EPA02L)+(CYDAD*DIA)+(CYDRD*DRUDD*DRFLX5*EPA43)+
                    //  ((CYDTD*DTFLX5)*DELEDD)+(CFYP*PB)+(CFYR*RB)+CYRB;

                }
                for (var beta1 = -20; beta1 <= 20; beta1 += 5)
                {
                    var RABET = Math.Abs(beta1) / DTOR;
                    //  ROLLING  MOMENT
                    //var DTFLX1 = 0.9750;
                    //var DRFLX1 = 0.85;
                    CML1.Add(alpha, beta1, GetEPA02S(beta1) * (-0.00238235 - (0.046126235 * RAL) + (0.10553168 * RAL * RAL)
                    + (0.10541585 * Math.Pow(RAL, 3)) - (0.40254765 * Math.Pow(RAL, 4))
                    + (0.32530491 * Math.Pow(RAL, 5)) - (0.08496121 * Math.Pow(RAL, 6))
                    + (0.00112288 * Math.Pow(RAL, 7)) - (0.05940477 * RABET * RAL) -
                    (0.07356236 * RABET) - (0.00550119 * RABET * RABET) + (0.00326191 * Math.Pow(RABET, 3))));
                }
                if (RAL < 0.29671)
                    CMLP.Add(alpha, -0.24963201 - (0.03106297 * RAL) + (0.12430631 * RAL * RAL)
                    - (8.95274618 * Math.Pow(RAL, 3)) + (100.33109929 * Math.Pow(RAL, 4))
                    + (275.70069578 * Math.Pow(RAL, 5)) - (1178.83425699 * Math.Pow(RAL, 6))
                    - (2102.66811522 * Math.Pow(RAL, 7)) + (2274.89785551 * Math.Pow(RAL, 8)));
                if (RAL >= 0.29671 && RAL < 0.34907)
                    CMLP.Add(alpha, -0.1635261 - (3.77847099 * (RAL - 0.29671001)) + (147.47639465
                    * Math.Pow(RAL - 0.29671001, 2)) - (1295.94799805 * Math.Pow(RAL - 0.29671001, 3)));

                if (RAL >= 0.34907)
                    CMLP.Add(alpha, -1.37120291 + (7.06112182 * RAL) - (13.57010422 * RAL * RAL) + (11.21323850 * Math.Pow(RAL, 3))
                    - (4.26789425 * Math.Pow(RAL, 4)) + (0.6237381 * Math.Pow(RAL, 5)));
                if (alpha > -25)
                {
                    if (RAL < 0.7854)
                        CMLR.Add(alpha, 0.03515391 + (0.59296381 * RAL) + (2.27456302 * RAL * RAL)
                        - (3.8097803 * Math.Pow(RAL, 3))
                        - (45.83162842 * Math.Pow(RAL, 4)) + (55.31669213 * Math.Pow(RAL, 5)) +
                        (194.29237485 * Math.Pow(RAL, 6)) - (393.22969953 * Math.Pow(RAL, 7)) + (192.20860739 * Math.Pow(RAL, 8)));
                    else
                        if (RAL >= 0.7854 && RAL <= 0.87266)
                            CMLR.Add(alpha, 0.0925579071 - (0.6000000238 * (RAL - 0.7853999734))
                            + (1.3515939713 * (Math.Pow(RAL - 0.7853999734, 2)))
                            + (29.0733299255 * (Math.Pow(RAL - 0.7853999734, 3))));
                        else if (RAL > 0.87266)
                            CMLR.Add(alpha, -311.126041 + (1457.23391042 * RAL) - (2680.19461944 * RAL * RAL) +
                            (2361.44914738 * Math.Pow(RAL, 3)) - (893.83567263 * Math.Pow(RAL, 4)) + (68.23501924 *
                            Math.Pow(RAL, 6)) - (1.72572994 * Math.Pow(RAL, 9)));
                    //var CML = (CML1*EPA02S);
                }

                if (RAL < 0)
                    DCLB.Add(alpha, -0.00006);
                else if (RAL >= 0 && RAL <= 0.209434)
                    DCLB.Add(alpha, -0.00006 + (0.0041035078 * RAL * RAL) - (0.0130618699 * Math.Pow(RAL, 3)));
                else if (RAL > 0.209434)
                    DCLB.Add(alpha, 0);

                if (alpha >= -15)
                {
                    var A = alpha;
                    var F1 = -4.33509 + A * (-0.141624 + A * (0.0946448 + A * (-0.00798481
                      + A * (-0.00168344 + A * (0.000260037 + A * (6.64054e-6 + A * (
                      -2.20055e-6 + A * (-2.74413e-8 + A * (7.14476e-9 + A *
                      2.07046e-10)))))))));
                    var F2 = -302.567 + A * (106.288 + A * (-14.7034 + A * (1.02524 + A * (-0.0393491
                    + A * (0.00084082 + A * (-9.365e-6 + A * 4.2355e-8))))));
                    var F3 = 1724.99 + A * (-158.944 + A * (5.59729 + A * (-0.0949624 + A * (
                   0.000779066 + A * (-2.47982e-6)))));
                    var R1 = 1.0 - 0.75 * Math.Pow(A - 10.0, 2) + 0.25 * Math.Pow(A - 10.0, 3);
                    var R2 = 1.0 - R1;
                    var R3 = 1.0 - 7.5 * Math.Pow(A - 40, 2) / 62.5 + Math.Pow(A - 40.0, 3) / 62.5;
                    var R4 = 1.0 - R3;
                    if (A < 10)
                    {
                        CMMQ.Add(alpha, F1);
                        //CMMQ.Add(alpha,-3.8386262+(13.54661297*RAL)+(402.53011559*RAL*RAL)
                        //-(6660.95327122*Math.Pow(RAL,3))-(62257.89908743*Math.Pow(RAL,4))
                        //+(261526.10242329*Math.Pow(RAL,5))
                        //+(2177190.33155227*Math.Pow(RAL,6))-(703575.13709062*Math.Pow(RAL,7))-
                        //(20725000.34643054*Math.Pow(RAL,8))-(27829700.53333645*Math.Pow(RAL,9)));
                    }
                    else if (A < 12)
                        CMMQ.Add(alpha, F1 * R2 + F2 * R2);
                    else if (A < 40)
                        CMMQ.Add(alpha, F2);
                    else if (A < 45)
                        CMMQ.Add(alpha, F2 * R3 + F3 * R4);
                    else
                        CMMQ.Add(alpha, F3);
                }
                //CNM (yaw moment)
                var DTFLX3 = 0.9750e0;
                var DRFLX3 = 0.890e0;

                //for (var aileron = -30; aileron <= 30; aileron += 5)
                //{
                //    var DAILA = Math.Abs(aileron);
                //    CNDAD.Add(alpha, aileron, 0.00008228887 - (0.00014015 * RAL) - (0.0013493 * RAL * RAL) +
                //    (0.00020487 * Math.Pow(RAL, 3)) + (0.00561241 * Math.Pow(RAL, 4))
                //    - (0.00634392 * Math.Pow(RAL, 5))
                //    + (0.00193323 * Math.Pow(RAL, 6)) - (2.05815E-17 * (RAL * DAILA)) + (3.794816E-17 * +Math.Pow(DAILA, 3)));
                //}
                CNDAD.Add(alpha, 0.00008228887 - (0.00014015 * RAL) - (0.0013493 * RAL * RAL) +
                (0.00020487 * Math.Pow(RAL, 3)) + (0.00561241 * Math.Pow(RAL, 4))
                - (0.00634392 * Math.Pow(RAL, 5))
                + (0.00193323 * Math.Pow(RAL, 6))
                    //- (2.05815E-17 * (RAL * DAILA)) + (3.794816E-17 * +Math.Pow(DAILA, 3))
                );
            }


            //// EPA02  IS  A MULTIPLIER THAT  ADJUSTS  (CFY1,CML1,CMN1) 
            //// BY  CHANGING THAT COEFICIENT  SIGN  DEPEDENT  ON  THE  SIGN
            //// OF  THE  SIDESLIP  ANGLE  (BETA).  IF  BETA  IS  NEGTIVE  THEN
            //// EPAO2=-1.0.  IF BETA IS POSITIVE THEN EA02=1.0.  SINCE TH!S
            //// FUNCTION  IS  DISCONTINUOUS  AT  THE  ORIGIN  A  CUBIC  SPLINE  HAS
            ////  BEEN EMPLOYED REPRESENT THIS  FUNCTION  
            //for (var beta = -20; beta <= 20; beta += 1)
            //{
            //    if (beta < -1.0)
            //        EPA02S.Add(beta, -1);
            //    else if (beta >= -1.0 && beta <= 1)
            //        EPA02S.Add(beta, -1.0 + (1.5 * Math.Pow(beta + 1, 2)) -
            //         (0.5 * Math.Pow(beta + 1, 3.0)));
            //    else if (beta > 1)
            //        EPA02S.Add(beta, 1);

            //    if (beta < -5.0)
            //        EPA02L.Add(beta, -1);
            //    if (beta >= -5.0 && beta <= 5)
            //        EPA02L.Add(beta,
            //        -1 + (0.06 * (Math.Pow(beta + 5, 2))) -
            //        (0.0040 * (Math.Pow(beta + 5, 3))));
            //    if (beta > 5)
            //        EPA02L.Add(beta, 1);

            //}


             return aerodata;
        }

        private static double GetEPA02S(int beta1)
        {
            double EPA02S = 0;
            if (beta1 < -1.0)
                EPA02S = -1;
            else if (beta1 >= -1.0 && beta1 <= 1)
                EPA02S = -1.0 + (1.5 * Math.Pow(beta1 + 1, 2)) - (0.5 * Math.Pow(beta1 + 1, 3.0));
            else if (beta1 > 1)
                EPA02S = 1;
            return EPA02S;
        }

        private static double GetEPA02L(int beta1)
        {
            double EPA02L = 0;
            if (beta1 < -5.0)
                EPA02L = -1;
            else if (beta1 >= -5.0 && beta1 <= 5)
                EPA02L = -1 + (0.06 * (Math.Pow(beta1 + 5, 2))) - (0.0040 * (Math.Pow(beta1 + 5, 3)));
            else if (beta1 > 5)
                EPA02L = 1;
            return EPA02L;
        }

        private static int inc(int alpha)
        {
            if (alpha < -10) return alpha + 1;
            if (alpha > 35) return alpha + 5;
            return alpha + 1;
        }
        private static int incbeta(int beta)
        {
            if (beta < -10) return beta + 10;
            if (beta > 10) return beta + 10;
            return beta + 5;
        }

    }
}