import Tabulator from 'tabulator-tables';

const AUDIO_BUTTON_HTML = `
  <button class="mdc-icon-button audio-button" aria-hidden="true" aria-pressed="false">
    <i class="material-icons mdc-icon-button__icon mdc-icon-button__icon--on">stop</i>
    <i class="material-icons mdc-icon-button__icon">play_arrow</i>
  </button>
`;

const ICON_BUTTON_ON = 'mdc-icon-button--on';

interface AudioSegment {
  url: string;
  startOffset: number;
  endOffset: number;
}

export class Table {
  private readonly table: Tabulator;
  private readonly audio = new Audio();
  private playingCellElement?: HTMLElement;
  private lastPcnt: number = 0;
  private audioProgressTimer?: number;

  constructor(id: string, params: any, columns: any[], data: any) {
    this.table = new Tabulator(`#${id}`, {
      ...params,
      layoutColumnsOnNewData: true,
      columns: this.getColumnDefs(columns),
      data
    });
  }

  setColumns(columns: any[]): void {
    this.table.setColumns(this.getColumnDefs(columns));
  }

  setData(data: any): void {
    this.table.setData(data);
  }

  setPlaybackRate(rate: number): void {
    this.audio.defaultPlaybackRate = rate;
    this.audio.playbackRate = rate;
  }

  private getColumnDefs(columns: any[]): Tabulator.ColumnDefinition[] {
    for (let i = 0; i < columns.length; i++) {
      const column = columns[i];
      if (column.formatter === 'audio') {
        column.formatter = () => AUDIO_BUTTON_HTML;
        column.cellClick = (_e: any, cell: Tabulator.CellComponent) => {
          const elem = cell.getElement();
          const buttonElem = elem.firstElementChild!;
          const isOn = buttonElem.classList.contains(ICON_BUTTON_ON);
          if (isOn) {
            buttonElem.classList.remove(ICON_BUTTON_ON);
            this.stopAudio();
          } else {
            if (this.playingCellElement != null) {
              this.playingCellElement.firstElementChild!.classList.remove(ICON_BUTTON_ON);
              this.stopAudio();
            }
            buttonElem.classList.add(ICON_BUTTON_ON);
            const audioSegment: AudioSegment = cell.getRow().getData()[cell.getField()];
            this.playAudio(elem, audioSegment.url, audioSegment.startOffset, audioSegment.endOffset);
          }
          elem.setAttribute('aria-pressed', `${!isOn}`)
        };
      }
    }
    return columns;
  }

  private playAudio(cellElement: HTMLElement, url: string, start: number, end: number): void {
    this.stopAudio();

    this.audio.src = url;
    this.audio.currentTime = start;
    this.playingCellElement = cellElement;
    this.playingCellElement.classList.add('animate-progress');
    this.audioProgressTimer = setInterval(() => {
      if (this.playingCellElement == null) {
        return;
      }

      if (this.audio.currentTime >= end) {
        this.audio.currentTime = start;
        if (this.audio.paused) {
          this.audio.play();
        }
      }
      const pcnt = Math.round(((this.audio.currentTime - start) / (end - start)) * 100);
      if (pcnt < this.lastPcnt) {
        this.playingCellElement.classList.remove('animate-progress');
      } else {
        this.playingCellElement.classList.add('animate-progress');
      }
      this.playingCellElement.style.backgroundSize = `${pcnt}% 100%`;
      this.lastPcnt = pcnt;
    }, 250);
    this.audio.play();
  }

  private stopAudio(): void {
    if (this.playingCellElement != null) {
      this.playingCellElement.classList.remove('animate-progress');
      this.playingCellElement.style.backgroundSize = '';
      this.playingCellElement = undefined;
    }
    if (this.audioProgressTimer != null) {
      clearInterval(this.audioProgressTimer);
      this.audioProgressTimer = undefined;
    }
    this.lastPcnt = 0;
    if (!this.audio.paused) {
      this.audio.pause();
    }
  }
}