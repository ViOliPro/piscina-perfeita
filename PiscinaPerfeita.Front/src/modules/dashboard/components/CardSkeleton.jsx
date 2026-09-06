import { Card } from "../../../components/ui/index.jsx";

export function CardSkeleton({ title = "…", lines = 4 }) {
  return (
    <Card title={title}>
      <div style={{ display: "flex", flexDirection: "column", gap: 10 }}>
        {Array.from({ length: lines }).map((_, i) => (
          <div
            key={i}
            style={{
              height: 12,
              borderRadius: 6,
              background: "rgba(107,140,174,.15)",
              width: i === lines - 1 ? "60%" : "100%",
            }}
          />
        ))}
      </div>
    </Card>
  );
}
