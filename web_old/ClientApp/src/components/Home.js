import React, { Component } from 'react';
import { setChonkyDefaults } from 'chonky';
import { ChonkyIconFA } from 'chonky-icon-fontawesome';
import { VFSReadOnly } from './chonky/VFSReadOnly';
import { RunScript } from './RunScript';

setChonkyDefaults({ iconComponent: ChonkyIconFA });

export class Home extends Component {
  static displayName = Home.name;

  render () {
    return (
      <div>
        <h1>Wellcome to AutoCheck!</h1>
        <div id="step-1">
          <p>Select the AutoCheck's YAML script that you want to launch, double clicking will execute it:</p>
          <VFSReadOnly instanceId="1" />
        </div>
        <div id="step-2">
          <RunScript />
        </div>        
      </div>
    );
  }
}