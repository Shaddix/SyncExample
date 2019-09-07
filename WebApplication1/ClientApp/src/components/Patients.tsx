import * as React from 'react';
import {PatientDto, PatientsClient} from "../api/client";
import {connection, SyncComponent} from "./SyncComponent";
import { Link } from 'react-router-dom';

type State = {
  loading: boolean;
  patients: PatientDto[];
};
type Props = {};

export class Patients extends React.Component<Props, State> {

  private patientsClient = new PatientsClient();


  constructor(props: Props) {
    super(props);
    this.state = {
      patients: [],
      loading: true,
    };
    this.loadData = this.loadData.bind(this);
  }

  async componentDidMount() {
    await this.loadData();
  }

  async loadData() {
    const patients = await this.patientsClient.get('');
    this.setState({
      patients: patients,
      loading: false,
    });

  }

  render() {
    const {loading, patients} = this.state;

    if (loading) {
      return <div>loading...</div>;
    }

    return (
        <div>
          <SyncComponent onUpdate={this.loadData}
                         entityType="patient"
                         keys={patients.map(patient => patient.id!)}
          >
            <ul>
              {patients.map(patient => <li key={patient.id}>{patient.id}: <Link to={`/details/${patient.id}`}>{patient.name}</Link></li>)}
            </ul>
          </SyncComponent>
          <button onClick={() => this.sendMessage()}>Send message via http</button>
          <button onClick={() => this.sendMessageViaSignalr()}>Send message via SignalR</button>
        </div>
    );
  }

  sendMessage() {
    this.patientsClient.get2('123123123123');
  }

  async sendMessageViaSignalr() {
    await connection.invoke('SendMessage', '123qwe');
  }
}
