using System;
using System.Collections.Generic;
using Core.Vectors;

namespace Core.NumericalIntegration;

public static class NumericalIntegrationMethods
{
    public static IEnumerable<QuadratureNode<Vector2D>> GaussQuadratureTriangleOrder3()
    {
        var v1 = new Vector2D(1.0 / 3.0, 1.0 / 3.0);
        var v2 = new Vector2D(3.0 / 5.0, 1.0 / 5.0);
        var v3 = new Vector2D(1.0 / 5.0, 3.0 / 5.0);
        var v4 = new Vector2D(1.0 / 5.0, 1.0 / 5.0);

        const double w1 = -9.0 / 32.0;
        const double w2 = 25.0 / 96.0;

        Vector2D[] points = { v1, v2, v3, v4 };
        double[] weights = { w1, w2, w2, w2 };
        for (int i = 0; i < 4; i++)
            yield return new QuadratureNode<Vector2D>(points[i], weights[i]);
    }

    public static IEnumerable<QuadratureNode<Vector2D>> GaussQuadratureTriangleOrder5()
    {
        var v1 = new Vector2D(1.0 / 3.0, 1.0 / 3.0);
        var v2 = new Vector2D(0.059715871789770, 0.470142064105115);
        var v3 = new Vector2D(0.470142064105115, 0.059715871789770);
        var v4 = new Vector2D(0.470142064105115, 0.470142064105115);
        var v5 = new Vector2D(0.797426985353087, 0.101286507323456);
        var v6 = new Vector2D(0.101286507323456, 0.797426985353087);
        var v7 = new Vector2D(0.101286507323456, 0.101286507323456);

        const double w1 = 0.1125;
        const double w2 = 0.066197076394253;
        const double w3 = 0.0629695902724135;

        Vector2D[] points = { v1, v2, v3, v4, v5, v6, v7 };
        double[] weights = { w1, w2, w2, w2, w3, w3, w3 };
        for (int i = 0; i < 7; i++)
            yield return new QuadratureNode<Vector2D>(points[i], weights[i]);
    }

    public static IEnumerable<QuadratureNode<Vector2D>> GaussQuadratureTriangleOrder6()
    {
        var v1 = new Vector2D(0.873821971016996, 0.063089014491502);
        var v2 = new Vector2D(0.063089014491502, 0.873821971016996);
        var v3 = new Vector2D(0.063089014491502, 0.063089014491502);
        var v4 = new Vector2D(0.501426509658179, 0.249286745170910);
        var v5 = new Vector2D(0.249286745170910, 0.501426509658179);
        var v6 = new Vector2D(0.249286745170910, 0.249286745170910);
        var v7 = new Vector2D(0.636502499121399, 0.310352451033785);
        var v8 = new Vector2D(0.310352451033785, 0.636502499121399);
        var v9 = new Vector2D(0.636502499121399, 0.053145049844816);
        var v10 = new Vector2D(0.053145049844816, 0.636502499121399);
        var v11 = new Vector2D(0.310352451033785, 0.053145049844816);
        var v12 = new Vector2D(0.053145049844816, 0.310352451033785);

        const double w1 = 0.0254224531851035;
        const double w2 = 0.0583931378631895;
        const double w3 = 0.041425537809187;

        Vector2D[] points = [v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12];
        double[] weights = [w1, w1, w1, w2, w2, w2, w3, w3, w3, w3, w3, w3];

        for (int i = 0; i < 12; i++)
            yield return new QuadratureNode<Vector2D>(points[i], weights[i]);
    }

    public static IEnumerable<QuadratureNode<Vector2D>> GaussQuadratureTriangleOrder9()
    {
        var v1 = new Vector2D(0.0451890097844, 0.0451890097844);
        var v2 = new Vector2D(0.0451890097844, 0.9096219804312);
        var v3 = new Vector2D(0.9096219804312, 0.0451890097844);
        var v4 = new Vector2D(0.7475124727339, 0.0304243617288);
        var v5 = new Vector2D(0.2220631655373, 0.0304243617288);
        var v6 = new Vector2D(0.7475124727339, 0.2220631655373);
        var v7 = new Vector2D(0.2220631655373, 0.7475124727339);
        var v8 = new Vector2D(0.0304243617288, 0.7475124727339);
        var v9 = new Vector2D(0.0304243617288, 0.2220631655373);
        var v10 = new Vector2D(0.1369912012649, 0.2182900709714);
        var v11 = new Vector2D(0.6447187277637, 0.2182900709714);
        var v12 = new Vector2D(0.1369912012649, 0.6447187277637);
        var v13 = new Vector2D(0.2182900709714, 0.6447187277637);
        var v14 = new Vector2D(0.2182900709714, 0.1369912012649);
        var v15 = new Vector2D(0.6447187277637, 0.1369912012649);
        var v16 = new Vector2D(0.0369603304334, 0.4815198347833);
        var v17 = new Vector2D(0.4815198347833, 0.0369603304334);
        var v18 = new Vector2D(0.4815198347833, 0.4815198347833);
        var v19 = new Vector2D(0.4036039798179, 0.1927920403641);
        var v20 = new Vector2D(0.4036039798179, 0.4036039798179);
        var v21 = new Vector2D(0.1927920403641, 0.4036039798179);

        const double w1 = 0.0519871420646 * 0.25;
        const double w2 = 0.0707034101784 * 0.25;
        const double w3 = 0.0909390760952 * 0.25;
        const double w4 = 0.1032344051380 * 0.25;
        const double w5 = 0.1881601469167 * 0.25;

        Vector2D[] points = [v1, v2, v3, v4, v5, v6, v7, v8, v9, v10, v11, v12, v13, v14, v15, v16, v17, v18, 
            v19, v20, v21]; 
        double[] weights = [w1, w1, w1, w2, w2, w2, w2, w2, w2, w3, w3, w3, w3, w3, w3, w4, w4, w4, w5, w5, w5];
        
        for (int i = 0; i < 21; i++)
            yield return new QuadratureNode<Vector2D>(points[i], weights[i]);
    }

    public static IEnumerable<QuadratureNode<double>> GaussQuadrature1DOrder3()
    {
        double[] points = { -1.0 / Math.Sqrt(3), 1.0 / Math.Sqrt(3) };
        double[] weights = { 1.0, 1.0 };

        for (int i = 0; i < 2; i++)
            yield return new QuadratureNode<double>((points[i] + 1) * 0.5, 0.5 * weights[i]);
    }

    public static IEnumerable<QuadratureNode<double>> GaussQuadrature1DOrder5()
    {
        double[] points = { -Math.Sqrt(0.6), 0.0, Math.Sqrt(0.6) };
        double[] weights = { 5.0 / 9.0, 8.0 / 9.0, 5.0 / 9.0 };

        for (int i = 0; i < 3; i++)
            yield return new QuadratureNode<double>((points[i] + 1) * 0.5, 0.5 * weights[i]);
    }

    public static IEnumerable<QuadratureNode<double>> GaussQuadrature1DOrder7()
    {
        const double p1 = 0.339981043584856;
        const double p2 = 0.861136311594052;

        const double w1 = 0.652145154862546;
        const double w2 = 0.347854845137453;

        double[] points = [-p1, p1, -p2, p2];
        double[] weights = [w1, w1, w2, w2];

        for (int i = 0; i < 4; i++)
            yield return new QuadratureNode<double>((points[i] + 1) * 0.5, 0.5 * weights[i]);
    }

    public static IEnumerable<QuadratureNode<double>> GaussQuadrature1DOrder9()
    {
        const double p1 = 0.9061798459386640;
        const double p2 = 0.5384693101056831;
        const double w1 = 0.2369268850561891;
        const double w2 = 0.4786286704993665;
        const double w3 = 0.5688888888888889;

        double[] points = { -p1, -p2, 0.0, p2, p1 };
        double[] weights = { w1, w2, w3, w2, w1 };

        for (int i = 0; i < 5; i++)
            yield return new QuadratureNode<double>((points[i] + 1) * 0.5, 0.5 * weights[i]);
    }

    public static double NumericalValueIntegralOnEdge(QuadratureNodes<double> nodes, Func<double, double> func)
    {
        var value = 0.0;

        for (int k = 0; k < nodes.Nodes.Length; k++)
            value += nodes.Nodes[k].Weight * func(nodes.Nodes[k].Node);

        return value;
    }
}