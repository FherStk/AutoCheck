import React, { Component } from 'react';

export class RunScript extends Component {
  static displayName = RunScript.name;

  constructor(props) {
    super(props);
    this.state = { forecasts: [], loading: true };
    this.reload = this.reload.bind(this);
  }

  componentDidMount() {
    this.populateWeatherData();
  }

  reload() {    
    this.populateWeatherData();
  }

  static renderForecastsTable(forecasts) {
    return (
      <table className='table table-striped' aria-labelledby="tabelLabel">
        <thead>
          <tr>
            <th>Date</th>
            <th>Temp. (C)</th>
            <th>Temp. (F)</th>
            <th>Summary</th>
          </tr>
        </thead>
        <tbody>
          {forecasts.map(forecast =>
            <tr key={forecast.date}>
              <td>{forecast.date}</td>
              <td>{forecast.temperatureC}</td>
              <td>{forecast.temperatureF}</td>
              <td>{forecast.summary}</td>
            </tr>
          )}
        </tbody>
      </table>
    );
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : RunScript.renderForecastsTable(this.state.forecasts);

    return (
      <div>
        <p>This component demonstrates fetching data from the server.</p>
        <button className="btn btn-primary" onClick={this.reload}>Reload</button>
        {contents}
      </div>
    );
  }

  async populateWeatherData() {
    const response = await fetch('weatherforecast');
    const data = await response.json();
    this.setState({ forecasts: data, loading: false });
  }
}
