import * as React from "react";
import * as signalR from "@aspnet/signalr";
import {HubConnectionState} from "@aspnet/signalr";

type SyncProps = {
  children: any;
  onUpdate: () => any;
  entityType: string;
  keys: number[];
};
export const connection = new signalR.HubConnectionBuilder()
    .withUrl("/dataUpdates")
    .withAutomaticReconnect()
    .build();


export class SyncComponent extends React.Component<SyncProps, {}> {

  async componentDidMount() {
    connection.on('DataUpdated', (entityType, id) => {
      this.props.onUpdate();
    });

    if (connection.state === HubConnectionState.Disconnected) {
      await connection.start();
    }
    this.subscribe();
  }

  subscribe() {
    if (this.props.keys.length > 0) {
      connection.invoke('SubscribeToDataUpdates', this.props.entityType, this.props.keys);
    }
  }

  componentDidUpdate(prevProps: Readonly<SyncProps>, prevState: Readonly<{}>, snapshot?: any): void {
    if (prevProps.keys.join(';') !== (this.props.keys || []).join(';')) {
      this.subscribe();
    }
  }

  render() {
    return this.props.children;
  }
}