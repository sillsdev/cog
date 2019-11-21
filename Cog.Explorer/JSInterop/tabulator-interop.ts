import Tabulator from 'tabulator-tables';

export class TabulatorInterop {
  private readonly tables = new Map<string, Tabulator>();

  createTable(id: string, columnsStr: string, dataStr: string): void {
    const columns = JSON.parse(columnsStr) as Tabulator.ColumnDefinition[];
    const data = JSON.parse(dataStr);
    const table = new Tabulator(`#${id}`, {
      height: '100%',
      columns,
      data,
      layoutColumnsOnNewData: true,
      columnVertAlign: 'bottom'
    });
    this.tables.set(id, table);
  }

  setColumns(id: string, columnsStr: string): void {
    const table = this.tables.get(id);
    if (table != null) {
      const columns = JSON.parse(columnsStr) as Tabulator.ColumnDefinition[];
      table.setColumns(columns);
    }
  }

  setData(id: string, dataStr: string): void {
    const table = this.tables.get(id);
    if (table != null) {
      const data = JSON.parse(dataStr);
      table.setData(data);
    }
  }

  disposeTable(id: string): void {
    this.tables.delete(id);
  }
}
