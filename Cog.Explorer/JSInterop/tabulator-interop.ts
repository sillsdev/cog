import { Table } from './table';

export class TabulatorInterop {
  private readonly tables = new Map<string, Table>();

  createTable(id: string, paramsStr: string, columnsStr: string, dataStr: string): void {
    const table = new Table(id, JSON.parse(paramsStr), JSON.parse(columnsStr), JSON.parse(dataStr));
    this.tables.set(id, table);
  }

  setColumns(id: string, columnsStr: string): void {
    const table = this.tables.get(id);
    if (table != null) {
      table.setColumns(JSON.parse(columnsStr));
    }
  }

  setData(id: string, dataStr: string): void {
    const table = this.tables.get(id);
    if (table != null) {
      table.setData(JSON.parse(dataStr));
    }
  }

  setPlaybackRate(id: string, rate: number): void {
    const table = this.tables.get(id);
    if (table != null) {
      table.setPlaybackRate(rate);
    }
  }

  disposeTable(id: string): void {
    this.tables.delete(id);
  }
}
