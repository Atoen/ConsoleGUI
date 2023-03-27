using System.Diagnostics.Contracts;

namespace ConsoleGUI;

public record struct Vector(int X, int Y)
{
    public static Vector Zero => new(0, 0);

    public static Vector Up => new(0, -1);
    public static Vector Down => new(0, 1);
    public static Vector Left => new(-1, 0);
    public static Vector Right => new(1, 0);

    [Pure]
    public Vector ExpandTo(Vector other) => new(Math.Max(X, other.X), Math.Max(Y, other.Y));

    public static implicit operator Vector((int x, int y) t) => new(t.x, t.y);

    public static Vector operator +(Vector a, Vector b) => new(a.X + b.X, a.Y + b.Y);

    public static Vector operator -(Vector a, Vector b) => new(a.X - b.X, a.Y - b.Y);

    public static Vector operator /(Vector v, int i) => new(v.X / i, v.Y / i);

    public static Vector operator *(Vector v, int i) => new(v.X * i, v.Y * i);

    public static Vector operator -(Vector v) => new(-v.X, -v.Y);

    public static Vector operator +(Vector v) => v;

    public override string ToString() => $"({X} {Y})";
}
