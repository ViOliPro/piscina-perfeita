import { Component } from "react";
import { ErrorMessage, Button } from "./index.jsx";

export class ErrorBoundary extends Component {
  state = { error: null };

  static getDerivedStateFromError(error) {
    return { error };
  }

  handleRetry = () => {
    this.setState({ error: null });
    this.props.onReset?.();
  };

  render() {
    if (this.state.error) {
      return (
        this.props.fallback ?? (
          <div style={{ padding: 12 }}>
            <ErrorMessage
              message={this.state.error.message ?? "Erro ao carregar"}
            />
            <Button variant="ghost" size="sm" onClick={this.handleRetry}>
              Tentar de novo
            </Button>
          </div>
        )
      );
    }
    return this.props.children;
  }
}
