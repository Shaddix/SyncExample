import * as React from 'react';
import {PatientDto, PatientsClient} from "../api/client";
import {SyncComponent} from "./SyncComponent";

type LoadingData = {
  type: 'loading';
};
type LoadedData = {
  type: 'loaded';
  patient: PatientDto;
};

type State = {
  data: LoadingData | LoadedData;
};
type Props = {
  patientId: number;
};

export class PatientDetails extends React.Component<Props, State> {

  private patientsClient = new PatientsClient();

  constructor(props: Props) {
    super(props);
    this.state = {
      data: {type: 'loading'},
    };
    this.loadData = this.loadData.bind(this);
  }

  async componentDidMount() {
    await this.loadData();
  }

  render() {
    const {data} = this.state;

    if (data.type === "loading") {
      return <div>loading...</div>;
    }

    return (
        <SyncComponent onUpdate={this.loadData}
                       entityType="patient"
                       keys={[data.patient.id]}
        >
          <div>Name: {data.patient.name}</div>
          <div>Id: {data.patient.id}</div>
          <div>Days used: {data.patient.daysUsed}</div>
        </SyncComponent>
    );
  }

  async loadData() {
    const patient = await this.patientsClient.getDetails(this.props.patientId);
    this.setState({
      data: {type: "loaded", patient: patient},
    });
  }


}
