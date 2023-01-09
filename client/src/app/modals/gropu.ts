export interface Group {
  name: string;
  connections: Connection[];
}

export interface Connection {
  connenctionId: string;
  username: string;
}
